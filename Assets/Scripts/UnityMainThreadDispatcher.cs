using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChzzAPI
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        public static UnityMainThreadDispatcher Instance { get; private set; }
        
        private readonly Queue<Action> _executionQueue = new Queue<Action>();
        private readonly object _lockObject = new object();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            lock (_lockObject)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(Action action)
        {
            lock (_lockObject)
            {
                _executionQueue.Enqueue(action);
            }
        }

        public void Enqueue(IEnumerator action)
        {
            lock (_lockObject)
            {
                _executionQueue.Enqueue(() => StartCoroutine(action));
            }
        }

        public void Enqueue<T>(Action<T> action, T param)
        {
            lock (_lockObject)
            {
                _executionQueue.Enqueue(() => action(param));
            }
        }

        public void Enqueue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            lock (_lockObject)
            {
                _executionQueue.Enqueue(() => action(param1, param2));
            }
        }
    }
} 