using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisRx
{
    public interface IRedisDictionary<T> : IDictionary<string, T>
    {
        bool Fill();
        Task CommitChanges();
        IDictionary<string, T> Unwrap();
    }

    public class RedisDictionary<T> : IRedisDictionary<T>
    {
        public RedisDictionary(string key)
        {
            
        } 

        public Task CommitChanges()
        {
            return Task.FromResult(new object());
        }

        public void Add(string key, T value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public ICollection<string> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out T value)
        {
            throw new NotImplementedException();
        }

        public ICollection<T> Values
        {
            get { throw new NotImplementedException(); }
        }

        public T this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<string, T> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Fill()
        {
            throw new NotImplementedException();
        }


        public IDictionary<string, T> Unwrap()
        {
            throw new NotImplementedException();
        }
    }
}
