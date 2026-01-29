using HappyHarvest;
using UnityEngine;

public class ItemSuelto : MonoBehaviour
{
    [SerializeField] int cantidad;
    [SerializeField] int ID;
    [SerializeField] InventoryController inv;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L)) InstanceRandomItem();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inv.AddItem(ID, cantidad);
        }
    }

    void InstanceRandomItem()
    {
        
        inv.AddItem(Random.Range(1, 9), 1);
    }

}
