// AimArm.cs
using UnityEngine;

public class AimArm : MonoBehaviour
{
    [Tooltip("Si tu mano/arma mira a la derecha por defecto, dej� (1,1,1).")]
    public Vector3 normalScale = Vector3.one;

    [Tooltip("Si tu sprite queda mirando al rev�s, invert� el valor: (-1,1,1) o (1,-1,1).")]
    public Vector3 flippedScale = new Vector3(-1, 1, 1);

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouse - transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Apunta el "eje X" (right) del objeto hacia el mouse
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip visual cuando pasa por detr�s (izquierda)
        bool lookingLeft = (angle > 90f || angle < -90f);

        // Opci�n A: escalar (seguro)
        transform.localScale = lookingLeft ? flippedScale : normalScale;

        // Opci�n B (alternativa): sr.flipY = lookingLeft;
    }
}
