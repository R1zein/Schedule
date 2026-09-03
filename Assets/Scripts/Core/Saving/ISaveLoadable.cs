namespace Core.Saving
{
    /// <summary>
    /// Сущность с сохраняемым состоянием. Регистрируется в <see cref="SaveLoadService"/> сама
    /// и владеет своим куском сейва: сервис знает только ключ и строку, что внутри — дело сущности.
    /// </summary>
    public interface ISaveLoadable
    {
        /// <summary>Ключ раздела в файле. Строка, а не тип: переименование класса не должно ломать сейвы.</summary>
        string Key { get; }

        /// <summary>Снимок состояния — обычный [Serializable] класс. null — «мне нечего сохранять».</summary>
        object SaveState();

        /// <summary>Вызывается в момент регистрации. Своего раздела в файле может не быть — тогда остаёмся на дефолтах.</summary>
        void LoadState(SaveFile file);
    }
}
