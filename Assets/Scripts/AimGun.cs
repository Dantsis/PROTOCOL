using UnityEngine;

public class AimGun : MonoBehaviour
{
    [Header("Referencias")]
    public SpriteRenderer handSR;
    public SpriteRenderer gunSR;

    void Update()
    {
        // mouse → mundo
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // Dirección y ángulo
        Vector2 dir = (mouseWorld - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Rotar el pivote del brazo
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Si apunta a la izquierda, espejamos sprites en Y para que no queden "al revés"
        bool lookingLeft = angle > 90f || angle < -90f;
        if (handSR) handSR.flipY = lookingLeft;
        if (gunSR) gunSR.flipY = lookingLeft;

        // Opcional: si el flipY te corre la mano/arma, ajustá su posición local derecha/izquierda:
        // var lp = gunSR.transform.localPosition;
        // lp.y = lookingLeft ? -Mathf.Abs(lp.y) : Mathf.Abs(lp.y);
        // gunSR.transform.localPosition = lp;
    }
}
