using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.ComponentModel;
using System;
using System.Xml.Schema;

public class Container : MonoBehaviour
{
    [System.Serializable]
    public class ContainerItem
    {
        public System.Guid Id;
        public string Name;
        public int Maximum;

        private int amountTaken;
        public ContainerItem(){
            Id = System.Guid.NewGuid();
            }
        public int Remaining
        {
            get
            {
                return Maximum - amountTaken;
            }
        }

       public int Get(int value)
        {
            //throw new NotImplementedException();
            if (amountTaken + value > Maximum)
            {
                int toMuch = (amountTaken + value) - Maximum;
                amountTaken = Maximum;
                return value - toMuch;
            }

            amountTaken += value;
            return value;
        }

        public void Set(int amount)
        {
            amountTaken -= amount;
            if (amountTaken < 0)
                amountTaken = 0;
        }
    }
     public List<ContainerItem> items;
    public event System.Action OnContainerReady;
    private void Awake()
    {
      items  = new List<ContainerItem>();
        OnContainerReady?.Invoke();
    }
    public System.Guid Add(string name,int maximum)
    {
        items.Add(new ContainerItem
        {
            Maximum=maximum,
            Name=name
        });
        return items.Last().Id;
    }


    public void Put(string name,int amount)
    {
        var containerItem = items.Where(x => x.Name == name).FirstOrDefault();
        if (containerItem == null)
            return;

        containerItem.Set(amount);
    }
    public int TakeFromContainer(System.Guid id,int amount)
    {
        var containerItem = GetContainerItem(id);
        if (containerItem == null)
        {
            return -1;
        }
       return containerItem.Get(amount);
    }
    public int GetAmountRemaining(System.Guid id)
    {
        var containerItem = GetContainerItem(id);
        if (containerItem == null)
            return -1;
        return containerItem.Remaining;
    }
    private ContainerItem GetContainerItem(System.Guid id)
    {
        var containerItem = items.Where(x => x.Id == id).FirstOrDefault();
        if (containerItem == null)
            return null;
        return containerItem;
    }
}
