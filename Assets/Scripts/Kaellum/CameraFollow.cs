using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Leave empty to auto-find the PlayerMovement in the scene.")]
    [SerializeField] private Transform _target;
    [SerializeField] private Rigidbody2D _targetRb;
    [SerializeField] private Vector2 _offset = new Vector2(0f, 1f);

    [Header("Smoothing")]
    [SerializeField] private float _horizontalSmoothTime = 0.15f;
    [SerializeField] private float _verticalSmoothTime = 0.25f;

    [Header("Look Ahead")]
    [SerializeField] private float _lookAheadDistance = 3f;
    [SerializeField] private float _lookAheadSmoothTime = 0.35f;
    [SerializeField] private float _lookAheadMinSpeed = 0.5f;

    [Header("Vertical Dead Zone")]
    [Tooltip("Total height the player can move before the camera follows vertically.")]
    [SerializeField] private float _deadZoneHeight = 2f;
    [Tooltip("Seconds the player must be vertically still before the camera re-centres on them.")]
    [SerializeField] private float _recenterDelay = 0.25f;
    [SerializeField] private float _recenterSmoothTime = 0.4f;

    [Header("Fall Look Down")]
    [SerializeField] private float _fallLookDownDistance = 3f;
    [SerializeField] private float _fallSpeedThreshold = 10f;
    [SerializeField] private float _fallLookDownSmoothTime = 0.3f;

    [Header("Bounds (optional)")]
    [SerializeField] private bool _useBounds = false;
    [SerializeField] private Vector2 _minBounds = new Vector2(-50f, -20f);
    [SerializeField] private Vector2 _maxBounds = new Vector2(50f, 20f);

    private Camera _cam;

    private float _lookAhead;
    private float _lookAheadVel;
    private float _lookDown;
    private float _lookDownVel;
    private float _focusY;
    private float _recenterVel;
    private float _idleTimer;
    private float _xVel;
    private float _yVel;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (_target == null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                _target = player.transform;
            }
        }

        if (_target == null)
        {
            Debug.LogWarning("CameraFollow: no target assigned or found.", this);
            enabled = false;
            return;
        }

        if (_targetRb == null)
        {
            _targetRb = _target.GetComponent<Rigidbody2D>();
        }

        SnapToTarget();
    }

    private void LateUpdate()
    {
        Vector2 vel = _targetRb != null ? _targetRb.linearVelocity : Vector2.zero;

       
        float targetLookAhead = 0f;
        if (Mathf.Abs(vel.x) > _lookAheadMinSpeed)
        {
            targetLookAhead = Mathf.Sign(vel.x) * _lookAheadDistance;
        }
        _lookAhead = Mathf.SmoothDamp(_lookAhead, targetLookAhead, ref _lookAheadVel, _lookAheadSmoothTime);

        
        float targetLookDown = vel.y < -_fallSpeedThreshold ? -_fallLookDownDistance : 0f;
        _lookDown = Mathf.SmoothDamp(_lookDown, targetLookDown, ref _lookDownVel, _fallLookDownSmoothTime);

       
        float playerY = _target.position.y + _offset.y;
        float halfZone = _deadZoneHeight * 0.5f;
        float diff = playerY - _focusY;

        if (Mathf.Abs(diff) > halfZone)
        {
            _focusY = playerY - Mathf.Sign(diff) * halfZone;
            _idleTimer = 0f;
        }
        else if (Mathf.Abs(vel.y) < 0.05f)
        {
            // Standing still vertically (e.g. landed on a platform): ease back to centre
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _recenterDelay)
            {
                _focusY = Mathf.SmoothDamp(_focusY, playerY, ref _recenterVel, _recenterSmoothTime);
            }
        }
        else
        {
            _idleTimer = 0f;
        }

        // Final desired position
        float desiredX = _target.position.x + _offset.x + _lookAhead;
        float desiredY = _focusY + _lookDown;

        float newX = Mathf.SmoothDamp(transform.position.x, desiredX, ref _xVel, _horizontalSmoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, desiredY, ref _yVel, _verticalSmoothTime);

        Vector3 pos = new Vector3(newX, newY, transform.position.z);
        transform.position = _useBounds ? ClampToBounds(pos) : pos;
    }

    private Vector3 ClampToBounds(Vector3 pos)
    {
        float halfHeight = _cam.orthographicSize;
        float halfWidth = halfHeight * _cam.aspect;

        float minX = _minBounds.x + halfWidth;
        float maxX = _maxBounds.x - halfWidth;
        float minY = _minBounds.y + halfHeight;
        float maxY = _maxBounds.y - halfHeight;

        // If the level is smaller than the view, just centre on it
        pos.x = minX > maxX ? (_minBounds.x + _maxBounds.x) * 0.5f : Mathf.Clamp(pos.x, minX, maxX);
        pos.y = minY > maxY ? (_minBounds.y + _maxBounds.y) * 0.5f : Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }

    /// <summary>Instantly centre on the target (use after teleports or respawns).</summary>
    public void SnapToTarget()
    {
        if (_target == null) return;

        _lookAhead = _lookDown = 0f;
        _lookAheadVel = _lookDownVel = _recenterVel = _xVel = _yVel = 0f;
        _idleTimer = 0f;
        _focusY = _target.position.y + _offset.y;

        Vector3 pos = new Vector3(_target.position.x + _offset.x, _focusY, transform.position.z);
        transform.position = _useBounds ? ClampToBounds(pos) : pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (_target != null)
        {
            // Vertical dead zone
            Gizmos.color = Color.yellow;
            Vector3 centre = new Vector3(transform.position.x, _focusY, 0f);
            Gizmos.DrawWireCube(centre, new Vector3(2f, _deadZoneHeight, 0f));
        }

        if (_useBounds)
        {
            Gizmos.color = Color.cyan;
            Vector3 size = _maxBounds - _minBounds;
            Vector3 centre = (Vector3)(_minBounds + _maxBounds) * 0.5f;
            Gizmos.DrawWireCube(centre, size);
        }
    }
}
