using UnityEngine;

public static class Helper
{
    public static Vector3 RandomPositionRestricted(float y, Vector3 halfExtents)
    {
        LayerMask mask = LayerMask.GetMask("MLAgent-RayTest");
        Vector3 targetPos = Vector3.zero;
        targetPos.y = y;

        do
        {
            targetPos.x = Random.Range(-10, 10);
            targetPos.z = Random.Range(-8, 8);

            if ((targetPos.x > 1.5) && (targetPos.z < -2.6))
            {
                if (Random.Range((int)0, (int)1) == 0)
                {
                    targetPos.x = 1.5f;
                }
                else
                {
                    targetPos.z = -2.6f;
                }
            }


        } while (Physics.CheckBox(targetPos, halfExtents, Quaternion.identity, mask));

        return targetPos;
    }

    public static bool SetRandomLocalPositionUnRestricted(Transform transform, float y)
    {
        Vector3 halfExtents = Vector3.zero;
        int attempts = 0;
        bool stayInLoop = true;
        Vector3 targetPos = Vector3.zero;
        Vector3 worldPos = Vector3.zero;
        targetPos.y = y;

        LayerMask mask = LayerMask.GetMask("MLAgent-RayTest");
        do
        {
            if(++attempts >= 100) return false; //tried too many times - abort
            
            targetPos.x = Random.Range(-10.0f, 10.0f);
            targetPos.z = Random.Range(-8.0f, 8.0f);

            //if ((targetPos.x > 1.5) && (targetPos.z < -2.6))
            //{
            //    if (Random.Range((int)0, (int)1) == 0)
            //    {
            //        targetPos.x = 1.5f;
            //    }
            //    else
            //    {
            //        targetPos.z = -2.6f;
            //    }
            //}
            Physics.SyncTransforms();
            transform.localPosition = new Vector3(0, y, 0);
            worldPos = transform.TransformPoint(targetPos);
            halfExtents = transform.gameObject.GetComponent<Collider>().bounds.extents;
            Collider[] collider = Physics.OverlapBox(worldPos, halfExtents * 2.5f, Quaternion.identity, mask);
            if (collider.Length == 0) stayInLoop = false;


            //RaycastHit[] hits = Physics.BoxCastAll(targetPos, halfExtents*1.5f, Vector3.zero, Quaternion.identity, 0.01f, mask);
            //for (int i = 0; i < hits.Length; ++i)
            //{
            //    if (hits[i].collider.gameObject.layer == LayerMask.GetMask("MLAgent-RayTest")) result = true;
            //}
            
            //result = Physics.CheckBox(transform.position, halfExtents, Quaternion.identity, mask);
        } while (stayInLoop);

        if(!stayInLoop) //found an empty spot
        {
            transform.localPosition = targetPos;
        }
        else
        {
            //do not change position - assume we are already in a non-colliding one anyway
        }

        return true;
    }
}
