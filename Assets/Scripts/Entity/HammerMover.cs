using UnityEngine;
using Photon.Pun;

public class HammerMover : MonoBehaviourPun {
    public float speed = 6f, bounceHeight = 6f, terminalVelocity = 8.25f, vertspeed = 24f;
    public bool left;
    private Rigidbody2D body;
    private PhysicsEntity physics;

    void Start() {
        body = GetComponent<Rigidbody2D>();
        physics = GetComponent<PhysicsEntity>();

        object[] data = photonView.InstantiationData;
        left = (bool) data[0];
        if (data.Length > 1)
            speed += Mathf.Abs((float) data[1] / 3f);

        body.velocity = new Vector2(speed * (left ? -1 : 1), vertspeed);
    }
    void FixedUpdate() {
        if (GameManager.Instance && GameManager.Instance.gameover) {
            body.velocity = Vector2.zero;
            GetComponent<Animator>().enabled = false;
            body.isKinematic = true;
            return;
        }

        HandleCollision();

        float gravityInOneFrame = body.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime;
        body.velocity = new Vector2(speed * (left ? -1 : 1), Mathf.Max(-terminalVelocity, body.velocity.y));
    }
    void HandleCollision() {
        physics.UpdateCollisions();
    }

    void OnDestroy() {
    }

    [PunRPC]
    protected void Kill() {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(photonView);
    }

    void Bounce() {
        body.velocity = new Vector2(body.velocity.x, vertspeed);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (!photonView.IsMine)
            return;

        switch (collider.tag) {
        case "koopa":
        case "goomba": {
            KillableEntity en = collider.gameObject.GetComponentInParent<KillableEntity>();
            if (en.dead || en.Frozen)
                return;

            
            en.photonView.RPC("SpecialKill", RpcTarget.All, !left, false, 0);
            Bounce();
                    
            break;
        }
        case "frozencube": {
            FrozenCube fc = collider.gameObject.GetComponentInParent<FrozenCube>();
            if (fc.dead)
                return;
            // TODO: Stuff here

            
  
            fc.gameObject.GetComponent<FrozenCube>().photonView.RPC("Kill", RpcTarget.All);
            Bounce();
            break;
        }
        case "bulletbill": {
            KillableEntity bb = collider.gameObject.GetComponentInParent<BulletBillMover>();
            Bounce();

            break;
        }
        case "bobomb": {
            BobombWalk bobomb = collider.gameObject.GetComponentInParent<BobombWalk>();
                    if (bobomb.dead || bobomb.Frozen)
                        return;
                
                bobomb.photonView.RPC("Light", RpcTarget.All);
                bobomb.photonView.RPC("Kick", RpcTarget.All, body.position.x < bobomb.body.position.x, 0f, false);
                Bounce();
                    
            break;
        }
        case "piranhaplant": {
            KillableEntity killa = collider.gameObject.GetComponentInParent<KillableEntity>();
            if (killa.dead)
                return;
            AnimatorStateInfo asi = killa.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            if (asi.IsName("end") && asi.normalizedTime > 0.5f)
                return;
            killa.photonView.RPC("Kill", RpcTarget.All);
            Bounce();
            break;
        }
        }
    }
}
