namespace OtusSocialNetwork.Database.Entities;

public class UserEntity
{
    public string Id { get; set; }

    /// <summary>
    /// Имя
    /// </summary>
    public string First_name { get; set; }

    /// <summary>
    /// Фамилия
    /// </summary>
    public string Second_name { get; set; }

    /// <summary>
    /// Возраст
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Пол
    /// </summary>
    public string Sex { get; set; }

    /// <summary>
    /// Интересы
    /// </summary>
    public string Biography { get; set; }

    /// <summary>
    /// Город
    /// </summary>
    public string City { get; set; }
}
