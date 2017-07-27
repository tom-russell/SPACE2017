using System;

namespace Assets.Common
{
    public class Lazy<T>
    {
        private T _val;
        public bool LoadedValue { get; set; }

        public bool ReloadValue { get; set; }

        public T Value
        {
            get
            {
                Load();
                return _val;
            }
        }

        public void Load()
        {
            if (!LoadedValue || ReloadValue)
            {
                _val = _get();
                ReloadValue = false;
                LoadedValue = true;
            }
        }

        private Func<T> _get;

        public Lazy(Func<T> get)
        {
            _get = get;
        }
    }
}
