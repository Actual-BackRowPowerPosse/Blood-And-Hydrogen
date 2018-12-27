using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemSpawner : NetworkBehaviour {

    public GameObject bulletPrefab;
    public GameObject explosionPrefab;

    PlayerConnection connectionRef;

	// Use this for initialization
	void Start () {
        connectionRef = GetComponent<PlayerConnection>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void shootBullet()
    {
        CmdShootBullet();
    }

    public void shootTurret(float angle)
    {
        CmdShootTurret(angle);
    }


    [Command]
    void CmdShootTurret(float angle)
    {
        GameObject bulletObj = Instantiate(bulletPrefab);
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        bulletObj.transform.position = connectionRef.PlayerShipObj.transform.position;
        bulletObj.transform.rotation = rotation;
        NetworkServer.SpawnWithClientAuthority(bulletObj, connectionToClient);

    }


    [Command]
    void CmdShootBullet()
    {
        GameObject bulletObj = Instantiate(bulletPrefab);
        bulletObj.transform.position = connectionRef.PlayerShipObj.transform.position;
        bulletObj.transform.rotation = connectionRef.PlayerShipObj.transform.rotation;
        NetworkServer.SpawnWithClientAuthority(bulletObj, connectionToClient);
    }


}