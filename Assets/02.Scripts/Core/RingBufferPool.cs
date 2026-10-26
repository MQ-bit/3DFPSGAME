using System;
using UnityEngine;

// 고정 크기 링 버퍼 풀.
// - 반납(Release)이 없다. 한 바퀴 돌면 가장 오래된 것을 다시 꺼낸다.
// - 재생 중인 것을 끊어도 티가 안 나는 순수 연출(이펙트)용이다.
// - 게임플레이에 영향을 주는 객체는 ObjectPool 을 쓴다.
public class RingBufferPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly T[] _items;
    private readonly Transform _parent;
    private readonly Action<T> _onCreate;

    private int _next;

    public RingBufferPool(T prefab, int size, Transform parent = null, Action<T> onCreate = null)
    {
        _prefab = prefab;
        _items = new T[size];
        _parent = parent;
        _onCreate = onCreate;
    }

    // 지금까지 만들어진 항목 전체에 작업을 한다. (예: 모두 끄기)
    public void ForEachCreated(Action<T> action)
    {
        foreach (T item in _items)
        {
            if (item != null) action(item);
        }
    }

    // 다음 칸을 꺼낸다. 칸이 비어있거나 외부에서 파괴됐다면 새로 만든다.
    public T Get()
    {
        T item = _items[_next];
        if (item == null)
        {
            item = UnityEngine.Object.Instantiate(_prefab, _parent);
            _onCreate?.Invoke(item);
            _items[_next] = item;
        }

        _next = (_next + 1) % _items.Length;
        return item;
    }
}
