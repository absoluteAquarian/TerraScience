using System;
using System.Collections.Generic;

namespace TerraScience {
    public class Registry<T> {

        private Dictionary<string, T> REGISTRY = new Dictionary<string, T>();
        private Dictionary<T, string> INVERSE = new Dictionary<T, string>();
        public Registry() {
            
        }

        public void register(string key, T value) {
            if (REGISTRY[key] != null)
                throw new ArgumentException(String.Format("Tried to reassign key %s to %s, but it is already assigned to %s!",
                    key, value, REGISTRY[key])); 
            REGISTRY.Add(key, value);
            INVERSE.Add(value, key);
        }

        public T getObject(string name) {
            return this.REGISTRY[name];
        }

    }
}