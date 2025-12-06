using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager IN;

    public static event Action OnInventoryRefreshed;
}
