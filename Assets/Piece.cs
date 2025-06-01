using UnityEngine;

public class Piece : MonoBehaviour
{
    public Vector3 correctPosition;

    // 이후 드래그/트리거 이벤트 등에서 사용할 위치 비교용 메서드
    public bool IsInCorrectPosition(float threshold = 0.1f)
    {
        return Vector3.Distance(transform.position, correctPosition) < threshold;
    }
}
