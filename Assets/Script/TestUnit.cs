using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StateType_Move
{
    Idle,
    Moving
}
public class TestUnit : MonoBehaviour
{
    private bool bool_run = false;

    public float debug_counttime = 0f;

    public float findpathautotime = 2f;

    [SerializeField] protected StateType_Move statetype_move = StateType_Move.Idle;
    public float OneStepTime = 0.2f;
    protected Transform transform;
    private Vector2Int unitpos;
    public bool if_selectpath_auto = false;
    public Vector2Int targetpos;
    public List<AsNode> findpath;

    public bool bool_drawpath_debug = false;

    public Color color_debug_path = Color.green;
    public float CheckrotateDistance = 0.4f;
    void Awake()
    {
            bool_run = true;
}
    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        InitPosOnNodeNet();
    }

    // Update is called once per frame
    void Update()
    {
        debug_counttime += Time.deltaTime;
        if (if_selectpath_auto && statetype_move == StateType_Move.Idle && targetpos != unitpos)
        {
            Debug.Log("����Ѱ·����Ѱ��Ŀ��");
            SetMoveOnAsNodeNet(targetpos);
        }
        if (statetype_move == StateType_Move.Idle && findpath != null && findpath.Count > 0)
        {

            Debug.Log("����Э�̺����ƶ�");
            statetype_move = StateType_Move.Moving;
            Debug.Log("׼��Э�̺�������");
            //BasicPathMove();
            bool bool_ifcanmove = true;
            if (findpath == null)
            {
                Debug.Log("�����ĵ�λ��·���ƶ�����������һ����·��");
                bool_ifcanmove=false;
            }
            if (findpath.Count == 0)
            {
                Debug.Log("�ƶ�·��Ϊ��");
                bool_ifcanmove = false;
            }
            //��·��ȡ����һ���ƶ��ĵ�
            AsNode move_node = findpath[0];

            if (move_node.type == E_Node_Type.Stop)
            {
                Debug.Log("�ƶ������赲��ֹͣ�ƶ�");
                findpath = null;
                bool_ifcanmove = false;
            }
            if (bool_ifcanmove) 
            {
                //�Ƴ�ȷ���ƶ��ĵ�
                findpath.RemoveAt(0);
                //����꣬��ʼ�ƶ�


                //�Լ���λ���ó���ռ����һ��λ��
                AsGrid.Instance.nodes[unitpos.x, unitpos.y].type = E_Node_Type.Walk;//��֤ԭ��վ���ĵط�����������
                AsGrid.Instance.nodes[move_node.x, move_node.y].type = E_Node_Type.Stop;//��֤�¸�ռ�ݵĵط�����������

                //�߼�����ָ��λ��
                Debug.Log("�߼�����ָ��λ��");
                unitpos.x = move_node.x; unitpos.y = move_node.y;

                Vector3 movepoint_net = AsGrid.Instance.nodeslocationes[move_node.x, move_node.y];

                StartCoroutine(Move_IE(movepoint_net));
            }
        }
    }
    public void InitPosOnNodeNet()
    {
        // ����λ��������ת��Ϊ��Ƭ����
        Vector3Int unitPosN3 = AsGrid.Instance.map.WorldToCell(transform.position);
        // ������2D, �����ֶ�����Ƭ��Z�����Ϊ0
        Vector2Int unitPosN2 = new Vector2Int(unitPosN3.x, unitPosN3.y);
        Debug.Log("��⵽��λ����Ƭ����Ϊ" + unitPosN2);
        unitPosN2 = AsGrid.Instance.GridP_reflect_NetP(unitPosN2);
        Debug.Log("��⵽��λ����Ƭ����ת��Ϊ��������Ϊ" + unitPosN2);
        transform.position = AsGrid.Instance.nodeslocationes[unitPosN2.x, unitPosN2.y];
        unitpos = unitPosN2;
        AsGrid.Instance.nodes[unitPosN2.x, unitPosN2.y].type = E_Node_Type.Stop;//��֤��ɫվ���ĵط������˲�����
    }
    public void SetMoveOnAsNodeNet(Vector2Int trgPos)
    {
        // ����λ��������ת��Ϊ��Ƭ����
        Vector3Int unitPosN3 = AsGrid.Instance.map.WorldToCell(transform.position);
        // ������2D, �����ֶ�����Ƭ��Z�����Ϊ0
        Vector2Int unitPosN2 = new Vector2Int(unitPosN3.x, unitPosN3.y);
        Debug.Log("��⵽��λ����Ƭ����Ϊ" + unitPosN2);
        unitPosN2 = AsGrid.Instance.GridP_reflect_NetP(unitPosN2);
        Debug.Log("��⵽��λ����Ƭ����ת��Ϊ��������Ϊ" + unitPosN2);
        findpath = AsGrid.Instance.FindPath(unitPosN2, trgPos);
        if (findpath == null)
        {
            Debug.Log("������·����ȡ����������һ����·��");
            return;
        }
        //��˲�Ƶ���ʼλ�ã���׼����ȡ��·���Ŀ�ʼ��
        transform.position = AsGrid.Instance.nodeslocationes[unitPosN2.x, unitPosN2.y];
        findpath.RemoveAt(0);
    }
    public IEnumerator Move_IE(Vector3 movepoint_net)
    {
        //ʵ�ʶ���ָ��λ��\
        Debug.Log("ʵ�ʶ���ָ��λ��"+movepoint_net);

        while (transform.position != movepoint_net)
        {
            transform.position = Vector3.MoveTowards(transform.position, movepoint_net, 10 * Time.deltaTime);
            yield return 0;
        }
        statetype_move = StateType_Move.Idle;
        /*
         *         float t = 0;
            *for (; ; )
        {
            Debug.Log("�ƶ���"+t);
            t += Time.deltaTime;
            float a = t / OneStepTime;
            transform.position = Vector3.Lerp(transform.position, movepoint_net, a);
            if (a >= 1.0f)
            {
                Debug.Log("Э�̺���ִ����BasicPathMove�󣬽��ƶ�״̬��Ϊ����" + "����Э�̺�����ʱΪ" + t);
                statetype_move = StateType_Move.Idle;
                yield break;
            }
            Debug.Log("�ƶ�һ���ֻ�");
            yield return null;
        }
        Debug.Log("һ��������");
        yield break;*/

    }
    private void OnDrawGizmos()
    {
        if (bool_drawpath_debug && findpath != null && findpath.Count >= 0)
        {
            Gizmos.color = color_debug_path;
            for (int i = 0; i + 1 < findpath.Count; i++)
            {

                AsNode node = findpath[i];
                AsNode node1 = findpath[i + 1];
                Gizmos.DrawLine(AsGrid.Instance.nodeslocationes[node.x, node.y], AsGrid.Instance.nodeslocationes[node1.x, node1.y]);
            }
            Vector3 startpos = AsGrid.Instance.nodeslocationes[unitpos.x, unitpos.y];
            Vector3 endpos = AsGrid.Instance.nodeslocationes[targetpos.x, targetpos.y];
            Vector3 localpos = AsGrid.Instance.nodeslocationes[unitpos.x, unitpos.y];
            Gizmos.DrawWireSphere(startpos, CheckrotateDistance);
            Gizmos.DrawWireSphere(endpos, CheckrotateDistance + 0.1f);
            Gizmos.DrawWireSphere(localpos, CheckrotateDistance - 0.1f);
        }
    }
}
