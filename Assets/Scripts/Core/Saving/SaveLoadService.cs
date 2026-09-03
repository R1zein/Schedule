using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Saving
{
    /// <summary>
    /// Собирает все <see cref="ISaveLoadable"/> и гоняет их через один файл.
    /// О содержимом сущностей не знает — каждая отдаёт свой раздел под своим ключом.
    /// </summary>
    public class SaveLoadService : IInitializable, IDisposable
    {
        private readonly List<ISaveLoadable> _savables = new();

        private SaveFile _file;
        private bool _loaded;

        public void Initialize()
        {
            // Сохраняемся на выходе и на уходе в фон: правки, которые не бросают событий
            // (название урока, время), иначе не доедут до диска.
            Application.quitting += Save;
            Application.focusChanged += OnFocusChanged;
        }

        public void Dispose()
        {
            Application.quitting -= Save;
            Application.focusChanged -= OnFocusChanged;
        }

        /// <summary>
        /// Сущность регистрируется сама в своём Initialize и тут же получает состояние.
        /// Поэтому порядок вызова Initialize в контейнере, который Zenject не гарантирует,
        /// перестаёт что-либо значить: данные приходят ровно в тот момент, когда сущность готова.
        /// </summary>
        public void Register(ISaveLoadable savable)
        {
            if (_savables.Contains(savable))
            {
                Debug.LogWarning($"[SaveLoad] Повторная регистрация: {savable.Key}");
                return;
            }

            EnsureLoaded();
            _savables.Add(savable);

            if (_file != null)
            {
                savable.LoadState(_file);
            }
        }

        public void Unregister(ISaveLoadable savable) => _savables.Remove(savable);

        /// <summary>
        /// Накапливаем в общий снапшот, а не пересоздаём: разделы сущностей, которых сейчас
        /// нет в контейнере, остаются в файле нетронутыми.
        /// </summary>
        public void Save()
        {
            if (_savables.Count == 0)
            {
                return;
            }

            _file ??= new SaveFile();

            foreach (ISaveLoadable savable in _savables)
            {
                if (savable.SaveState() is { } state)
                {
                    _file.SetState(state, savable.Key);
                }
            }

            _file.Write();
        }

        // Файл читаем один раз, при первой регистрации, а не в Initialize — по той же причине,
        // по которой сущности регистрируются сами: порядок Initialize неизвестен.
        private void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            _file = SaveFile.Read();
        }

        private void OnFocusChanged(bool focused)
        {
            if (!focused)
            {
                Save();
            }
        }
    }
}
