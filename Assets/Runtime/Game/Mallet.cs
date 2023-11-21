using UnityEngine;

public class Mallet : MonoBehaviour
{
    private float _speed = .26f;
    private int _direction = 0;
    private Vector2 _bounds = new Vector2(-4, 4);
    private Renderer _renderer;

    public float Speed { get => _speed; set => _speed = value; }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.enabled = false;
    }

    public void Show()
    {
        _renderer.enabled = true;
    }

    public void SetDirection(int direction)
    {
        _direction = direction;
    }

    public void Move()
    {
        if (_direction == 0)
        {
            return;
        }

        float distance = _speed * _direction;
        float endX = Mathf.Clamp(transform.position.x + distance, _bounds[0], _bounds[1]);

        if (endX != transform.position.x)
        {
            transform.position = new Vector3(endX, transform.position.y, transform.position.z);
        }
    }
}
