using UnityEngine;
using System.Collections;

public class ETFXProjectileScript : MonoBehaviour
{
    public GameObject impactParticle; // Effect spawned when projectile hits a collider
    public GameObject projectileParticle; // Effect attached to the gameobject as child
    public GameObject muzzleParticle; // Effect instantly spawned when gameobject is spawned

    [SerializeField]
    private float duration_ = 1.0f;

    [SerializeField]
    private float scale_ = 1.0f;

    private void Start()
    {
        duration_ = 0.5f;
        scale_ = 0.5f;
        this.GetComponent<Rigidbody>().useGravity = false;
        Destroy(this.GetComponent<SphereCollider>());

        ///////////

        projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.SetParent(transform);

        float a_projectile_scale = scale_ * 1.0f;

        projectileParticle.transform.localScale = new Vector3(a_projectile_scale, a_projectile_scale, a_projectile_scale);

        if (muzzleParticle)
        {
            muzzleParticle = Instantiate(muzzleParticle, transform.position, transform.rotation) as GameObject;
            muzzleParticle.transform.localScale = new Vector3(scale_, scale_, scale_);

            Destroy(muzzleParticle, 1.5f); // 2nd parameter is lifetime of effect in seconds
        }

        if(duration_ > 0.0f)
        {
            StartCoroutine(destroy_coroutine());
        }
    }

    private IEnumerator destroy_coroutine()
    {
        if (duration_ <= 0.0f) yield break;

        yield return new WaitForSeconds(duration_);

        GameObject impactP = Instantiate(impactParticle, transform.position, Quaternion.identity) as GameObject; // Spawns impact effect
        impactP.transform.localScale = new Vector3(scale_, scale_, scale_);

        ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>(); // Gets a list of particle systems, as we need to detach the trails
                                                                             //Component at [0] is that of the parent i.e. this object (if there is any)
        for (int i = 1; i < trails.Length; i++) // Loop to cycle through found particle systems
        {
            ParticleSystem trail = trails[i];

            if (trail.gameObject.name.Contains("Trail"))
            {
                trail.transform.SetParent(null); // Detaches the trail from the projectile
                Destroy(trail.gameObject, 2f); // Removes the trail after seconds
            }
        }

        Destroy(projectileParticle, 3f); // Removes particle effect after delay
        Destroy(impactP, 3.5f); // Removes impact effect after delay
        Destroy(gameObject); // Removes the projectile
    }

    private void FixedUpdate()
    {
        //if (GetComponent<Rigidbody>().velocity.magnitude != 0)
        //{
        //    transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity); // Sets rotation to look at direction of movement
        //}
    }
}