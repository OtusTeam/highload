# Репликация в Postgres

1. Создаем сеть, запоминаем адрес
    ```shell
    docker network create pgnet
    docker network inspect pgnet | grep Subnet # Запомнить маску сети
    ```

2. Поднимаем мастер
    ```shell
    docker run -dit -v "$PWD/volumes/pgmaster/:/var/lib/postgresql/data" -e POSTGRES_PASSWORD=pass -p "5432:5432" --restart=unless-stopped --network=pgnet --name=pgmaster postgres
    ```

3. Меняем postgresql.conf на мастере
    ```conf
    ssl = off
    wal_level = replica
    max_wal_senders = 4 # expected slave num
    ```

4. Подключаемся к мастеру и создаем пользователя для репликации
    ```shell
    docker exec -it pgmaster su - postgres -c psql
    create role replicator with login replication password 'pass';
    exit
    ```

5. Добавляем запись в `pgmaster/pg_hba.conf` с `subnet` с первого шага
```
host    replication     replicator       __SUBNET__          md5
```

6. Перезапустим мастер
```shell
docker restart pgmaster
```

7. Сделаем бэкап для реплик
```shell
docker exec -it pgmaster bash
mkdir /pgslave
pg_basebackup -h pgmaster -D /pgslave -U replicator -v -P --wal-method=stream
exit
```

8. Копируем директорию себе
```shell
docker cp pgmaster:/pgslave volumes/pgslave/
```

9. Создадим файл, чтобы реплика узнала, что она реплика
```shell
touch volumes/pgslave/standby.signal
```

10. Меняем `postgresql.conf` на реплике `pgslave`
```conf
primary_conninfo = 'host=pgmaster port=5432 user=replicator password=pass application_name=pgslave'
```

11. Запускаем реплику `pgslave`
    ```shell
    docker run -dit -v "$PWD/volumes/pgslave/:/var/lib/postgresql/data" -e POSTGRES_PASSWORD=pass -p "15432:5432" --network=pgnet --restart=unless-stopped --name=pgslave postgres
    ```

12. Запустим вторую реплику `pgasyncslave`
- скопируем бэкап
    ```shell
    docker cp pgmaster:/pgslave volumes/pgasyncslave/
    ```

- изменим настройки `pgasyncslave/postgresql.conf`
    ```conf
    primary_conninfo = 'host=pgmaster port=5432 user=replicator password=pass application_name=pgasyncslave'
    ```

- дадим знать что это реплика
    ```shell
    touch volumes/pgasyncslave/standby.signal
    ```

- запустим реплику `pgasyncslave`
    ```shell
    docker run -dit -v "$PWD/volumes/pgasyncslave/:/var/lib/postgresql/data" -e POSTGRES_PASSWORD=pass -p "25432:5432" --network=pgnet --restart=unless-stopped --name=pgasyncslave postgres
    ```

1.  Включаем сихнронную репликацию на `pgmaster`
- меняем файл `pgmaster/postgresql.conf`
    ```conf
    synchronous_commit = on
    synchronous_standby_names = 'FIRST 1 (pgslave, pgasyncslave)'
    ```

- перечитываем конфиг
    ```shell
    ```

1.  Включаем синхронную репликацию
2.  

synchronous_commit = on
synchronous_standby_names = 'FIRST 1 (pgslave, pgasyncslave)'

select pg_reload_conf();

15. Создадим тестовую таблицу и проверим репликацию

docker exec -it pgmaster su - postgres -c psql

select application_name, sync_state from pg_stat_replication;

create table test(id bigint primary key not null);
insert into test(id) values(1);

16. Запромоутим реплику pgslave

docker stop pgmaster

docker exec -it pgslave su - postgres -c psql

select * from pg_promote();

synchronous_commit = on
synchronous_standby_names = 'ANY 1 (pgmaster, pgasyncslave)'

17. Подключим вторую реплику к новому мастеру

primary_conninfo = 'host=pgslave port=5432 user=replicator password=pass application_name=pgasyncslave'


18. Восстановим мастер в качестве реплики

touch pgmaster/standby.signal

primary_conninfo = 'host=pgslave port=5432 user=replicator password=pass application_name=pgmaster'


19. Настроим логическую репликацию с текущего мастера (pgslave) на новый сервер

wal_level = logical

docker restart pgslave

20. Создадим публикацию

GRANT CONNECT ON DATABASE postgres TO replicator;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO replicator;
create publication pg_pub for table test;

21. Создадим новый сервер для логической репликации

docker run -dit -v $PWD/pgstandalone/:/var/lib/postgresql/data -e POSTGRES_PASSWORD=pass -p 35432:5432 --restart=unless-stopped --network=pgnet --name=pgstandalone postgres

22. Копируем файлы
docker exec -it pgslave su - postgres

pg_dumpall -U postgres -r -h pgslave -f /var/lib/postgresql/roles.dmp
pg_dump -U postgres -Fc -h pgslave -f /var/lib/postgresql/schema.dmp -s postgres


docker cp pgslave:/var/lib/postgresql/roles.dmp .
docker cp roles.dmp pgstandalone:/var/lib/postgresql/roles.dmp
docker cp pgslave:/var/lib/postgresql/schema.dmp .
docker cp schema.dmp pgstandalone:/var/lib/postgresql/schema.dmp


docker exec -it pgstandalone su - postgres
psql -f roles.dmp
pg_restore -d postgres -C schema.dmp

23. Создаем подписку

CREATE SUBSCRIPTION pg_sub CONNECTION 'host=pgslave port=5432 user=replicator password=pass dbname=postgres' PUBLICATION pg_pub;

24. Сделаем конфликт в данных

На sub:
insert into test values(9);

На pub:
insert into test values(9);

В логах видим:
2023-03-27 16:15:02.753 UTC [258] ERROR:  duplicate key value violates unique constraint "test_pkey"
2023-03-27 16:15:02.753 UTC [258] DETAIL:  Key (id)=(9) already exists.
2023-03-28 18:30:42.893 UTC [108] CONTEXT:  processing remote data for replication origin "pg_16395" during message type "INSERT" for replication target relation "public.test" in transaction 739, finished at 0/3026450

25. Исправляем конфликт

SELECT pg_replication_origin_advance('pg_16395', '0/3026451'::pg_lsn); <- message from log + 1
ALTER SUBSCRIPTION pg_sub ENABLE;
