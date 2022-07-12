using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileTypes
{
    Hill,
    Forest,
    Pasture,
    Field,
    Mountain,
    Desert,
}

public class Tile : MonoBehaviour
{
    public int index = -1;

    [HideInInspector] public List<Corner> myCorners;

    [HideInInspector] public TileTypes myTileType { get; private set; }
    [HideInInspector] public ResourceType myResourceType { get; private set; }

    [HideInInspector] public int myValue { get; private set; }
    [HideInInspector] public int myProbabilityOfRevenue;
    public float myDisturbanceFactor;

    [HideInInspector] public bool hasRobber;

    Vector3 actualPosition;
    Quaternion actualRotation;
    Rigidbody rb;

    bool hasBeenEnabled = false;

    void Awake()
    {
        MeshCollider collider = gameObject.AddComponent<MeshCollider>();
        collider.convex = true;

        rb = gameObject.AddComponent<Rigidbody>();
        rb.drag = 1f;
        rb.isKinematic = true;
    }

    void Start()
    {
        actualPosition = transform.position;
        actualRotation = transform.rotation;

        transform.position += Vector3.up * (15f + (1f * (index + 5)));

        transform.rotation = new Quaternion(GetSignedRange(0.25f, 0.3f), transform.rotation.y, GetSignedRange(0.25f, 0.3f), transform.rotation.w);
    }

    float GetSignedRange(float min, float max) {
        return (Random.value * Random.Range(min, max)) * (Random.value > 0.5 ? 1 : -1);
    }

    public void EnablePhysics()
    {
        rb.isKinematic = false;
        rb.velocity = Vector3.down * 2f;
        hasBeenEnabled = true;
    }

    void OnCollisionStay(Collision other)
    {
        if (other.collider.name == "Table")
        {
            StartCoroutine(MoveIntoPlace());
        }
    }

    IEnumerator MoveIntoPlace()
    {
        yield return new WaitUntil(() =>
        {
            return (rb.angularVelocity.magnitude < 1f && rb.velocity.magnitude < 1f) || rb.IsSleeping();
        });
        yield return new WaitForSeconds(0.5f * Random.value);
        rb.isKinematic = true;
        gameObject.GetComponent<MeshCollider>().isTrigger = true;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (hasBeenEnabled && rb.isKinematic)
        {
            Vector3 direction = (actualPosition - transform.position).normalized;
            transform.position = Vector3.Lerp(transform.position, actualPosition, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, actualRotation, Time.deltaTime * 5f);
        }
    }

    public void SetTile(float radius, TileTypes type, int value, int probability)
    {
        myTileType = type;
        myValue = value;
        myProbabilityOfRevenue = probability;
        myDisturbanceFactor = 0;

        switch (type)
        {
            case TileTypes.Desert:
                myResourceType = ResourceType.none;
                break;
            case TileTypes.Field:
                myResourceType = ResourceType.Grain;
                break;
            case TileTypes.Forest:
                myResourceType = ResourceType.Lumber;
                break;
            case TileTypes.Hill:
                myResourceType = ResourceType.Brick;
                break;
            case TileTypes.Mountain:
                myResourceType = ResourceType.Ore;
                break;
            case TileTypes.Pasture:
                myResourceType = ResourceType.Wool;
                break;
        }
    }
}
