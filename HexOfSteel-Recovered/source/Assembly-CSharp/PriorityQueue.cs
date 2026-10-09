using System.Collections.Generic;

public class PriorityQueue<T>
{
	private readonly List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

	public int Count => elements.Count;

	public void Enqueue(T p_item, float p_priority)
	{
		elements.Add(new KeyValuePair<T, float>(p_item, p_priority));
		HeapifyUp(elements.Count - 1);
	}

	public T Dequeue()
	{
		T key = elements[0].Key;
		elements[0] = elements[elements.Count - 1];
		elements.RemoveAt(elements.Count - 1);
		HeapifyDown(0);
		return key;
	}

	private void HeapifyUp(int index)
	{
		while (index > 0)
		{
			int num = (index - 1) / 2;
			if (!(elements[index].Value >= elements[num].Value))
			{
				Swap(index, num);
				index = num;
				continue;
			}
			break;
		}
	}

	private void HeapifyDown(int index)
	{
		while (index < elements.Count)
		{
			int num = 2 * index + 1;
			int num2 = 2 * index + 2;
			int num3 = index;
			if (num < elements.Count && elements[num].Value < elements[num3].Value)
			{
				num3 = num;
			}
			if (num2 < elements.Count && elements[num2].Value < elements[num3].Value)
			{
				num3 = num2;
			}
			if (num3 != index)
			{
				Swap(index, num3);
				index = num3;
				continue;
			}
			break;
		}
	}

	private void Swap(int indexA, int indexB)
	{
		KeyValuePair<T, float> value = elements[indexA];
		elements[indexA] = elements[indexB];
		elements[indexB] = value;
	}
}
