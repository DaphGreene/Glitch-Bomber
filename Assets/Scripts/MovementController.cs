using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.down;
    private bool isDead;
    public float speed = 5f;

    [Header("Input")]
    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

    [Header("Sprites")]
    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    private AnimatedSpriteRenderer activeSpriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;

        if (spriteRendererUp == null || spriteRendererDown == null ||
            spriteRendererLeft == null || spriteRendererRight == null || spriteRendererDeath == null)
        {
            Debug.LogWarning($"{name}: MovementController is missing one or more sprite renderer references.", this);
        }
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        float horizontalInput = 0f;
        if (Input.GetKey(inputLeft))
        {
            horizontalInput -= 1f;
        }
        if (Input.GetKey(inputRight))
        {
            horizontalInput += 1f;
        }

        float verticalInput = 0f;
        if (Input.GetKey(inputDown))
        {
            verticalInput -= 1f;
        }
        if (Input.GetKey(inputUp))
        {
            verticalInput += 1f;
        }

        Vector2 inputDirection = new Vector2(horizontalInput, verticalInput);
        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection.Normalize();
        }

        if (inputDirection == Vector2.zero)
        {
            SetDirection(Vector2.zero, activeSpriteRenderer);
            return;
        }

        SetDirection(inputDirection, GetSpriteRendererForDirection(inputDirection));
    }

    private AnimatedSpriteRenderer GetSpriteRendererForDirection(Vector2 inputDirection)
    {
        bool hasHorizontalInput = !Mathf.Approximately(inputDirection.x, 0f);
        bool hasVerticalInput = !Mathf.Approximately(inputDirection.y, 0f);

        if (hasHorizontalInput && hasVerticalInput)
        {
            if (inputDirection.y > 0f && activeSpriteRenderer == spriteRendererUp)
            {
                return spriteRendererUp;
            }
            if (inputDirection.y < 0f && activeSpriteRenderer == spriteRendererDown)
            {
                return spriteRendererDown;
            }
            if (inputDirection.x < 0f && activeSpriteRenderer == spriteRendererLeft)
            {
                return spriteRendererLeft;
            }
            if (inputDirection.x > 0f && activeSpriteRenderer == spriteRendererRight)
            {
                return spriteRendererRight;
            }
        }

        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))
        {
            return inputDirection.x > 0f ? spriteRendererRight : spriteRendererLeft;
        }

        if (inputDirection.y > 0f)
        {
            return spriteRendererUp;
        }

        return spriteRendererDown;
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        Vector2 position = rb.position;
        Vector2 translation = speed * Time.fixedDeltaTime * direction;

        rb.MovePosition(position + translation);
    }

    private void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        direction = newDirection;

        SetRendererEnabled(spriteRendererUp, spriteRenderer == spriteRendererUp);
        SetRendererEnabled(spriteRendererDown, spriteRenderer == spriteRendererDown);
        SetRendererEnabled(spriteRendererLeft, spriteRenderer == spriteRendererLeft);
        SetRendererEnabled(spriteRendererRight, spriteRenderer == spriteRendererRight);

        activeSpriteRenderer = spriteRenderer;
        if (activeSpriteRenderer != null)
        {
            activeSpriteRenderer.idle = direction == Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead)
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            DeathSequence();
        }
    }

    private void DeathSequence()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        BombController bombController = GetComponent<BombController>();
        if (bombController != null)
        {
            bombController.DisableBombPlacement();
        }

        SetRendererEnabled(spriteRendererUp, false);
        SetRendererEnabled(spriteRendererDown, false);
        SetRendererEnabled(spriteRendererLeft, false);
        SetRendererEnabled(spriteRendererRight, false);
        SetRendererEnabled(spriteRendererDeath, true);

        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    private void OnDeathSequenceEnded()
    {
        BombController bombController = GetComponent<BombController>();
        if (bombController != null && bombController.HasActiveBombs())
        {
            StartCoroutine(WaitForActiveBombsToResolve(bombController));
            return;
        }

        CompleteDeathSequence();
    }

    private IEnumerator WaitForActiveBombsToResolve(BombController bombController)
    {
        while (bombController != null && bombController.HasActiveBombs())
        {
            yield return null;
        }

        CompleteDeathSequence();
    }

    private void CompleteDeathSequence()
    {
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckWinState();
        }
    }

    private static void SetRendererEnabled(AnimatedSpriteRenderer renderer, bool isEnabled)
    {
        if (renderer != null)
        {
            renderer.enabled = isEnabled;
        }
    }
}
