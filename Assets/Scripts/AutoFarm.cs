using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// An automatic farm that produces resources on a timer.
/// </summary>
public class AutoFarm : MonoBehaviour
{
    public int ID { get => _id; }
    [Header("Identity")]
    [SerializeField] private int _id = -1;
    private static int s_farmCount = -1;

    /// <summary>
    /// Whether this farm has been unlocked.
    /// </summary>
    public bool Active = false;

    /// <summary>
    /// All <c>AutoFarms</c> connected to this <c>AutoFarm</c> instance.
    /// </summary>
    public ReadOnlyCollection<AutoFarm> Connections { get => _connections.AsReadOnly(); }
    /// <summary>
    /// All <c>AutoFarms</c> connected to this <c>AutoFarm</c> instance.
    /// </summary>
    [SerializeField] List<AutoFarm> _connections = new();

    public int Level { get => _level; }
    [Header("Resources")]
    [SerializeField] private int _level = -1;
    /// <summary>
    /// If the farm would gain no new resources from leveling up, it is max level.
    /// </summary>
    [HideInInspector] public bool IsMaxLevel { get => _level == _resourcesPerLevel.Count; }

    /// <summary>
    /// A list of all resources this farm would gain from leveling up, with index 0 being all resources gained
    /// at level 0, index 1 being all resources gained at level 1, etc...
    /// </summary>
    public ReadOnlyCollection<PerLevelResources> ResourcesPerLevel { get => _resourcesPerLevel.AsReadOnly(); }
    /// <summary>
    /// A list of all resources this farm would gain from leveling up, with index 0 being all resources gained
    /// at level 0, index 1 being all resources gained at level 1, etc...
    /// </summary>
    [SerializeField] private List<PerLevelResources> _resourcesPerLevel = new();

    /// <summary>
    /// All resources this farm is able to produce at its current level.
    /// </summary>
    [SerializeField] private List<ResourceData> _producableResources = new();

    /// <summary>
    /// How fast the farm produces resources in resource generation event per second.
    /// </summary>
    public float ProductionRate
    {
        get => _baseProductionRate +
        (Mathf.Pow(_employeeCount / GROWTH_LIMITER, _baseProductionRate * GROWTH_LIMITER) / _baseProductionRate);
    }
    
    [Header("Production Rates")]
    /// <summary>
    /// How fast the farm produces resources in resource generation event per second with no employees.
    /// </summary>
    [SerializeField][Range(0.5f, 2f)] private float _baseProductionRate = 0.5f;

    /// <summary>
    /// Used to increase the <see cref="ProductionRate"><c>ProductionRate</c></see> of this farm.
    /// </summary>
    public int EmployeeCount
    {
        get { return _employeeCount; }
        set { _employeeCount = (value < 0) ? 0 : value; }
    }
    /// <summary>
    /// Used to increase the <see cref="ProductionRate"><c>ProductionRate</c></see> of this farm.
    /// </summary>
    [SerializeField] private int _employeeCount = 0;

    /// <summary>
    /// Determines how sharpely <c>ProductionRate</c> scales with <c>EmployeeCount</c>.
    /// </summary>
    private const float GROWTH_LIMITER = 2f;

    /// <summary>
    /// Stores the coroutine respondible for producing resources on a timer.
    /// </summary>
    private Coroutine _produce = null;

    public void Awake()
    {
        _id = ++s_farmCount;
        EnsureGoodConnections(this);

        if (Active)
            LevelUp();
    }

    public void Start()
    {
        _produce = StartCoroutine(ProduceResources());
    }

    /// <summary>
    /// Increases the <c>Level</c> of this farm by one (if possible) and adds new resources this farm is able
    /// to produce.
    /// </summary>
    /// <returns>The <c>Level</c> of this farm after leveling up</returns>
    public int LevelUp()
    {
        if (IsMaxLevel)
            return _level;

        _level++;
        // Adds all resources that this farm gains by leveling up to the list of resources this farm can produce.
        foreach (ResourceData newResource in _resourcesPerLevel[_level].Resources)
        {
            // Checks whether this farm can already produce each resource gained, and if so, instead updates
            // the amount of that resource this farm produces at one time.
            bool duplicateResource = false;
            foreach (ResourceData currentResource in _producableResources)
            {
                if (currentResource.Resource == newResource.Resource)
                {
                    duplicateResource = true;
                    currentResource.AmountPer += newResource.AmountPer;
                    break;
                }
            }
            if (!duplicateResource)
                _producableResources.Add(newResource);
        }
        return _level;
    }

    /// <summary>
    /// Adds all available resources this farm can produce at a <c>ProductionRate</c> of resource generation
    /// event per second.
    /// </summary>
    IEnumerator ProduceResources()
    {
        while (Active)
        {
            yield return new WaitForSeconds(1f / ProductionRate);
            ResourceManager.s_Instance.IncreaseResources(_producableResources);
        }
    }

    /// <summary>
    /// Ensures all connections of a farm connect back to itself and is not connected to itself.
    /// </summary>
    /// <param name="target">The central farm whose connections are being checked</param>
    public void EnsureGoodConnections(AutoFarm target)
    {
        // Scrubs the connections list of this farm, deleting all null values and references to itself.
        // If this farm contains a connection to the target farm, and the target is not equal to itself,
        // the connection is bidirectional and the method is exited.
        bool targetFound = false;
        for (int i = 0; i < _connections.Count; i++)
        {
            if (_connections[i] == null)
            {
                _connections.RemoveAt(i--);
                continue;
            }
            if (_connections[i].ID == target.ID)
            {
                if (target.ID == this.ID)
                {
                    _connections.Remove(target);
                    i--;
                    continue;
                }
                targetFound = true;
            }
        }
        if (targetFound)
            return;

        // If this farm is the one to add, call this method on all of its connections.
        if (target.ID == this.ID)
        {
            foreach (AutoFarm farm in _connections)
                farm.EnsureGoodConnections(target);
        }
        // Otherwise add the target farm to the list of this farms connections.
        else
            _connections.Add(target);
    }
}
