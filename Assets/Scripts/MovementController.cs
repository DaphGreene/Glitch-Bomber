using UnityEngine;

public class MovementController : MonoBehaviour
{
    public new Rigidbody2D rigidbody { get; private set; }

    private void Awake() {
        rigidbody = GetComponent<Rigidbody2D>();
    }
}