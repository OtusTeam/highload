# OtusSocialNetwork. Инструкция по запуску

Запрос для создания таблиц лежит в папке `Postgres`.
Коллекция вызовов для Postman лежит в папке `Postman`.
Таблица пользователей заполнена из файла https://raw.githubusercontent.com/OtusTeam/highload/master/homework/people.csv

0. Все состояние базы данных хранится в папке `volumes`. Для того, чтобы запустить с нуля и заново создать базу данных нужно удалить папку `rm -rf volumes`.
1. Запускаем базу данных и API командой `docker-compose up -d`.
2. Ждем когда Postgres запустится, перезагрузится, создадутся таблицы.
3. Открываем Postman, создаем нового пользователя `qwerty` с паролем `qwerty`. Запрос `Register`. Получаем id пользователя. (В тестовой базе пользователь уже создан)
4. Логинимся с полученным id. Запрос `Login`. Получаем токен авторизации. Копируем его.
5. Получаем информацию о пользователе. Запрос `Get user by id`. На вкладке `Authorization` выбираем `Type: Bearer Token`, в поле `Token` вставляем скопированный токен с шага 4. В строке с адресом запроса после http://localhost:5000/user/get/ вставляем нужный id
6. Поиск пользователей. Запрос `Search`. На вкладке `Authorization` выбираем `Type: Bearer Token`, в поле `Token` вставляем скопированный токен с шага 4. На вкладке `Params` заполняем условия поиска. FirstName - фамилия, SecondName - имя. Если параметры не указывать, вернется 100 пользователей, отсортированных по id.

## Добавленный индекс

```sql
CREATE INDEX user_first_name_idx ON public."user" using btree (first_name text_pattern_ops,second_name text_pattern_ops) ;

--- После добавления индекса имеет смысл выполнить запрос ANALYZE
```

### Добавляем индекс через bash

```bash 
docker exec -it otus-db psql -U dbuser -d otusdb -c 'CREATE INDEX user_first_name_idx ON public."user" using btree (first_name text_pattern_ops,second_name text_pattern_ops) ; ANALYZE; '
```

### Удаляем индекс через bash

```bash 
docker exec -it otus-db psql -U dbuser -d otusdb -c 'DROP INDEX user_first_name_idx; ANALYZE;'
```

## Запрос для проверки

```sql
EXPLAIN ANALYSE
SELECT id, first_name, second_name, sex, age, city, biography
FROM public."user"
WHERE first_name LIKE 'Ива%' AND second_name LIKE 'Т%'
```

Без индекса

```
Gather  (cost=1000.00..22447.51 rows=25 width=196) (actual time=205.434..422.400 rows=700 loops=1)
  Workers Planned: 2
  Workers Launched: 2
  ->  Parallel Seq Scan on "user"  (cost=0.00..21445.01 rows=10 width=196) (actual time=195.593..408.058 rows=233 loops=3)
        Filter: (((first_name)::text ~~ 'Ива%'::text) AND ((second_name)::text ~~ 'Т%'::text))
        Rows Removed by Filter: 333100
Planning Time: 0.514 ms
Execution Time: 422.487 ms
```

После индекса

```
Bitmap Heap Scan on "user"  (cost=103.43..200.76 rows=25 width=196) (actual time=0.282..1.512 rows=700 loops=1)
  Filter: (((first_name)::text ~~ 'Ива%'::text) AND ((second_name)::text ~~ 'Т%'::text))
  Heap Blocks: exact=300
  ->  Bitmap Index Scan on user_first_name_idx  (cost=0.00..103.42 rows=25 width=0) (actual time=0.220..0.220 rows=700 loops=1)
        Index Cond: (((first_name)::text ~>=~ 'Ива'::text) AND ((first_name)::text ~<~ 'Ивб'::text) AND ((second_name)::text ~>=~ 'Т'::text) AND ((second_name)::text ~<~ 'У'::text))
Planning Time: 0.879 ms
Execution Time: 1.606 ms
```
