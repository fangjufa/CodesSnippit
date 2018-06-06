using UnityEngine;

/// <summary>
/// 计算绕某个轴的旋转，比如车轮，用鼠标控制它绕某个轴旋转。
/// </summary>
public class RotateCircle : MonoBehaviour {

    public float rate;

    private Vector3 lastDir = Vector3.zero;

    private Vector3 currentDir;

    private RaycastHit hit;
	
	// Update is called once per frame
	void Update () {

        if(Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == gameObject)
                {
                    if (lastDir == Vector3.zero)
                    {
                        lastDir = hit.point - transform.position;
                        currentDir = hit.point - transform.position;
                    }
                    else
                    {
                        lastDir = currentDir;
                        currentDir = hit.point - transform.position;

                        Rotate(lastDir, currentDir);
                    }
                }
                else
                    lastDir = Vector3.zero;
            }
            else
                lastDir = Vector3.zero;
        }
        if (Input.GetMouseButtonUp(0))
            lastDir = Vector3.zero;
	}

    /// <summary>
    /// left和right两个向量是从一个中心点出发的，
    /// 该函数计算从left向量旋转到right向量，旋转的度数。
    /// </summary>
    void Rotate(Vector3 leftDir,Vector3 rightDir)
    {
        //先求两个向量的叉积，因为角度计算是没有正负的，所以需要通过求叉积来计算正负。
        Vector3 cross = Vector3.Cross(leftDir, rightDir);
        //如果叉积向量非常小，则说明leftDir和rightDir非常接近，以至于旋转角度可忽略。
        if (Vector3.Magnitude(cross - Vector3.zero) < 0.0005f)
            return;
        //然后计算叉积得到的向量与中心轴的夹角。如果大于90°，说明与中心轴的方向相反，否则方向相同。
        float theta = GetAngleFromVectors(transform.forward, cross);
        //if(theta)
        Vector3 axis = transform.forward;
        if (theta > Mathf.PI / 2)
            axis = -axis;

        float rotateAngle = GetAngleFromVectors(leftDir, rightDir);
        transform.RotateAround(transform.position,axis, rotateAngle*rate);
        //Debug.Log(string.Format("theta:{0},rotateAngle:{1}，leftDir:{2},rightDir:{3}.",theta,rotateAngle,leftDir.ToString("0.000"),rightDir.ToString("0.000")));
    }

    /// <summary>
    /// 计算两个向量的夹角。
    /// 计算方法为theta = arccos(a.b/(|a||b|))
    /// </summary>
    float GetAngleFromVectors(Vector3 left,Vector3 right)
    {
        float abs = left.magnitude * right.magnitude;
        float dot = Vector3.Dot(left, right);
        return Mathf.Acos(dot / abs);
    }
}