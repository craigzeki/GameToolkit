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

    public static bool SetRandomLocalPositionUnRestricted(Transform transform, float y, Vector3 halfExtents)
    {
        int attempts = 0;
        Vector3 targetPos = Vector3.zero;
        targetPos.y = y;

        LayerMask mask = LayerMask.GetMask("MLAgent-RayTest");
        do
        {
            if(++attempts >= 10) return false; //tried too many times - abort
            
            targetPos.x = Random.Range(-10, 10);
            targetPos.z = Random.Range(-8, 8);

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
            transform.localPosition = targetPos;
        } while (Physics.CheckBox(transform.position, halfExtents, Quaternion.identity, mask));


        return true;
    }
}
