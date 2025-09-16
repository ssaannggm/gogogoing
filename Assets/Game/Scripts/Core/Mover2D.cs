// Mover2D.cs
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(UnitStats))]
public class Mover2D : MonoBehaviour
{
    Rigidbody2D rb; UnitStats st;
    void Awake() { rb = GetComponent<Rigidbody2D>(); st = GetComponent<UnitStats>(); }
    public void MoveTowards(Vector2 targetPos)
    {
        var dir = (targetPos - rb.position).normalized;
        rb.MovePosition(rb.position + dir * (st.CurrentStats.moveSpeed * Time.deltaTime));
    }
    public void Stop() { rb.linearVelocity = Vector2.zero; }
}
