using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central object responsible for storing and managing all resources.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager s_Instance = null;

    /// <summary>
    /// Generic currency used for transactions.
    /// </summary>
    public int Credits
    {
        get { return _credits; }
        set { _credits = (value < 0) ? 0 : value; }
    }
    /// <summary>
    /// Generic currency used for transactions.
    /// </summary>
    [SerializeField] private int _credits = 0;

    public void Awake()
    {
        // Singleton pattern.
        if (s_Instance == null)
            s_Instance = this;
        else
            Destroy(this);
    }

    /// <summary>
    /// Increases all given resources by an amount specified by their <c>ResourceData</c>
    /// </summary>
    /// <param name="resources">A list of all <c>ResourceData</c> that will be added to the bank</param>
    public void IncreaseResources(List<ResourceData> resources)
    {
        foreach (ResourceData r in resources)
        {
            switch (r.Resource)
            {
                case ResourceList.DIRT:
                    Debug.Log("DIRT serves as a placeholder and should not be included in final product.");
                    break;
                default:
                    Debug.Log($"GameManager resource list has not been updated to include {r.Resource}.");
                    break;
            }
        }
    }
}

/// <summary>
/// The list of all resources.
/// </summary>
public enum ResourceList
{
    /// <summary>
    /// Signifies the resource has not been set to anything.
    /// </summary>
    DIRT
}
