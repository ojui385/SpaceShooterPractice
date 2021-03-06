using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    // 몬스터와 주인공의 Transform 컴포넌트를 저장할 변수 선언
    private Transform monsterTr;
    private Transform playerTr;
    // 네비메시 에이전트 컴포넌트
    private NavMeshAgent agent;

    // 몬스터의 상태를 나타내는 열거형 변수 정의
    public enum STATE { IDLE, TRACE, ATTACK, DIE };

    // 몬스터의 상태를 저장하는 변수를 선언
    public STATE state = STATE.IDLE;

    // 추적 사정거리
    [Range(10, 50)]
    public float traceDist = 10.0f;
    // 공격 사정거리
    public float attackDist = 2.0f;

    // 몬스터의 사망여부 변수를 선언
    public bool isDie = false;

    // Animator 컴포넌트를 저장할 변수를 선언
    private Animator anim;

    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashDie = Animator.StringToHash("Die");

    private float hp = 100.0f;

    void Start()
    {
        monsterTr = GetComponent<Transform>();
        playerTr = GameObject.FindGameObjectWithTag("PLAYER")?.GetComponent<Transform>();

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        StartCoroutine(CheckState());
        StartCoroutine(MonsterAction());

    }

    // 몬스터의 상태값을 결정하는 코루틴
    IEnumerator CheckState()
    {
        while (isDie == false)
        {
            if (state == STATE.DIE)
            {
                yield break;    // 해당 코루틴을 정지시킴.
            }

            // 몬스터의 상태는 주인공 <--> 몬스터 거리
            float distance = Vector3.Distance(monsterTr.position, playerTr.position);

            // 공격 사정거리 이내인지 여부 판단
            if (distance <= attackDist)
            {
                state = STATE.ATTACK;
            }
            // 공격 사정거리 < 주인공 < 추적 사정거리
            else if (distance <= traceDist)
            {
                state = STATE.TRACE;
            }
            else
            {
                state = STATE.IDLE;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    // 몬스터의 상태값에 따라서 행동을 처리하는 코루틴
    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                case STATE.IDLE:
                    agent.isStopped = true;
                    anim.SetBool(hashTrace, false);
                    break;
                case STATE.TRACE:
                    agent.SetDestination(playerTr.position);
                    agent.isStopped = false;
                    anim.SetBool(hashTrace, true);
                    anim.SetBool(hashAttack, false);
                    break;
                case STATE.ATTACK:
                    anim.SetBool(hashAttack, true);
                    break;
                case STATE.DIE:
                    GetComponent<CapsuleCollider>().enabled = false;
                    agent.isStopped = true;
                    anim.SetTrigger(hashDie);
                    isDie = true;
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if(coll.collider.CompareTag("BULLET"))
        {
            // Hit reaction 애니메이션 실행
            anim.SetTrigger(hashHit);

            // Bullet 삭제
            Destroy(coll.gameObject);

            // 몬스터의 HP 차감
            hp -= 20.0f;
            if (hp <= 0.0f)
            {
                state = STATE.DIE;
            }
        }
    }

    void YouWin()
    {
        StopAllCoroutines();
        anim.SetTrigger("PlayerDie");
    }

}

// A*PathFinding 길찾기 알고리즘
// 네비게이션 시스템 (NavMesh) :    이동할 수 있는 영역과 이동할 수 없는 영역으로 데이터를  bake해야함.
