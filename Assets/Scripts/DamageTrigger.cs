using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour {
    public int damage = 1;

    private void OnTriggerStay2D(Collider2D col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player")) {
            Player p = col.gameObject.GetComponent<Player>();
            if (p.isHittable()) {
                if (transform.position.x < p.transform.position.x)
                    p.EnterHurtState(damage, true);
                else if (transform.position.x > p.transform.position.x) {
                    p.EnterHurtState(damage, false);
                }
            }
        }
    }

    /*
    private void OnTriggerStay2D(Collider2D col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player")) {
            Player p = col.gameObject.GetComponent<Player>();
            if (p.knockbackState == false) {
                p.TakeDamage(damage);
                if (transform.position.x < p.transform.position.x)
                    p.Knockback(true);
                else if (transform.position.x > p.transform.position.x) {
                    p.Knockback(false);
                }
            }
        }
    }
    */
}