using UnityEngine;

public class RotateSlowly : MonoBehaviour
{
    // ���b�̉�]���x�i�x�j
    public Vector3 speed = new Vector3(0f, 10f, 0f);
    // ���[�J����]�����[���h��]��
    public Space space = Space.Self;

    void Update()
    {
        // �t���[���Ɉˑ����Ȃ��悤�� deltaTime ���|����
        transform.Rotate(speed * Time.deltaTime, space);
    }
}
