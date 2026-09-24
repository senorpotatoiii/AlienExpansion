using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// Handles the storage and access of all <c>AutoFarms</c>.
/// </summary>
[ExecuteInEditMode]
public class FarmManager : MonoBehaviour
{
    public static FarmManager Instance;

    /// <summary>
    /// The farm that the player will start with.
    /// </summary>
    [SerializeField] private AutoFarm _startingFarm;

    /// <summary>
    /// The list of inactive farms able to be purchased on the basis of connections.
    /// </summary>
    public ReadOnlyCollection<AutoFarm> AvailableFarms { get => _availableFarms.AsReadOnly(); }
    /// <summary>
    /// The list of inactive farms able to be purchased on the basis of connections.
    /// </summary>
    private List<AutoFarm> _availableFarms = new();

    public void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void Start()
    {
        Initialize();
    }

    public void Update()
    {
        DebugConnections();
    }
    
    /// <summary>
    /// Sets the starting farm to active and populates the list of available farms.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws an exception if the starting farm has not
    /// been set in the editor.</exception>
    public void Initialize()
    {
        if (_startingFarm == null)
            throw new InvalidOperationException("Starting Farm has not been set in editor.");
        _startingFarm.Active = true;
        foreach (AutoFarm farm in _startingFarm.Connections)
            _availableFarms.Add(farm);
    }

    /// <summary>
    /// Updates the list of inactive available farms.
    /// </summary>
    /// <param name="toRemove">The farm that is being purchased or removed</param>
    /// <returns>False if the farm is <c>null</c> or not in the list of available farms, true otherwise.</returns>
    public bool UpdateAvailable(AutoFarm toRemove)
    {
        if (toRemove != null && _availableFarms.Contains(toRemove))
            _availableFarms.Remove(toRemove);
        else return false;

        foreach (AutoFarm farm in toRemove.Connections)
        {
            if (!_availableFarms.Contains(farm))
                _availableFarms.Add(farm);
        }

        return true;
    }

    /// <summary>
    /// Uses a breadth first traversal to add debug lines between every <c>AutoFarm</c>. Connections between two
    /// active farms are green, between an active and available farm are yellow, and between two inactive farms
    /// are red.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws an exception if the starting farm has not
    /// been set in the editor.</exception>
    public void DebugConnections()
    {
        if (_startingFarm == null)
            throw new InvalidOperationException("Starting Farm has not been set in editor.");
        
        Color connectionColor;
        List<AutoFarm> visited = new();
        Queue<AutoFarm> toVisit = new();
        toVisit.Enqueue(_startingFarm);

        while (toVisit.Count > 0)
        {
            if (visited.Contains(toVisit.Peek()))
            {
                toVisit.Dequeue();
                continue;
            }

            foreach (AutoFarm farm in toVisit.Peek().Connections)
            {
                if (visited.Contains(farm))
                    continue;

                if (farm.Active)
                    connectionColor = Color.green;
                else if (_availableFarms.Contains(farm))
                    connectionColor = Color.yellow;
                else
                    connectionColor = Color.red;
                Debug.DrawLine(toVisit.Peek().transform.position, farm.transform.position, connectionColor);

                toVisit.Enqueue(farm);
            }

            visited.Add(toVisit.Dequeue());
        }
    }
}
