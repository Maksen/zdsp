using UnityEngine;
using Zealot.Common.Entities;

public enum HomingType
{
    TargetPos,
    TargetEntity,
}

public class ParticleHoming : MonoBehaviour
{
    public HomingType homingType = HomingType.TargetEntity;
    public Transform target;
    public Transform bullet;
    public float bulletActiveDelay = 0.25f;
    public float bulletFlyDuration = 0.4f;
    public float minDistance = 1;

    bool mInitialized = false;
    float mTimeLeft = 0;
    Vector3 mOriginalLocalPos;
    Entity mTargetEntity;
    Vector3 mTargetPos = Vector3.zero;
    bool mHasTargetPos = false;
    bool mTargetSet = false;
    float mMinDistanceSqr = 1;
    float mTargetOffset;
    Quaternion mOriginalLocalRot;

    public void Init()
    {
        if (mInitialized)
            return;
        mInitialized = true;
        enabled = false;
        if (bullet == null)
            return;
        mOriginalLocalPos = bullet.localPosition;
        mOriginalLocalRot = bullet.localRotation;
        mTargetOffset = bullet.childCount > 0 ? bullet.GetChild(0).localPosition.z : bullet.localPosition.z;
        SetTarget_Editor();
    }

    // Update is called once per frame
    void Update()
    {
        if (mTimeLeft <= 0)
            return;
        float dt = Time.deltaTime;
        Vector3 _targetPos = GetTargetPos();

        if (mTimeLeft <= dt)
        {
            bullet.position = _targetPos;
            enabled = false;
            //bullet.gameObject.SetActive(false);
        }
        else
        {
            Vector3 mDir = _targetPos - bullet.position;
            if (mDir.sqrMagnitude <= mMinDistanceSqr)
            {
                bullet.position = _targetPos;
                enabled = false;
                //bullet.gameObject.SetActive(false);
            }
            else
            {
                Vector3 desiredForward = mDir;
                desiredForward.y = 0;
                desiredForward.Normalize();

                float t = dt / mTimeLeft;
                bullet.forward = Vector3.Slerp(bullet.forward, desiredForward, t);
                bullet.position += mDir * t;
            }
        }
        mTimeLeft -= dt;
    }

    private Vector3 GetTargetPos()
    {
        return mTargetEntity == null ? LerpByDistanceFromB(bullet.position, mTargetPos, mTargetOffset) : LerpByDistanceFromB(bullet.position, mTargetEntity.Position, mTargetOffset);
    }

    private Vector3 LerpByDistanceFromB(Vector3 A, Vector3 B, float x)
    {
        Vector3 P = B - x * Vector3.Normalize(B - A);
        return P;
    }

    public void SetTarget(Vector3? targetPos, Entity targetEntity)
    {
        mHasTargetPos = false;
        if (targetPos.HasValue)
        {
            mHasTargetPos = true;
            mTargetPos = targetPos.Value;
        }
        else
            mTargetPos = Vector3.zero;
        mTargetEntity = targetEntity;
        mTargetSet = true;
    }

    public void SetTarget_Editor()
    {
        if (!mTargetSet)
        {
            if (target != null)
            {
                mTargetPos = target.position;
                mHasTargetPos = true;
            }
            else
                mHasTargetPos = false;
        }
        mMinDistanceSqr = minDistance * minDistance;
    }

    public void Restart()
    {
        Init();
        enabled = false;
        mTimeLeft = 0;
        if (bullet == null)
            return;
        bullet.localPosition = mOriginalLocalPos;
        bullet.localRotation = mOriginalLocalRot;
        bullet.gameObject.SetActive(false);
        if (mTargetEntity == null && !mHasTargetPos)
            return;
        Vector3 _targetPos = GetTargetPos();
        Vector3 mDir = _targetPos - bullet.position;
        if (mDir.sqrMagnitude <= mMinDistanceSqr)
            return;
        Invoke("ShootBullet", bulletActiveDelay);
    }

    private void ShootBullet()
    {
        enabled = true;
        mTimeLeft = bulletFlyDuration;
        bullet.gameObject.SetActive(true);
        EmitterRef emitterRef = bullet.GetComponent<EmitterRef>();
        if (emitterRef != null)
            emitterRef.Restart();
        else
        {
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < particles.Length; i++)
            {
                ParticleSystem particle = particles[i];
                particle.Clear();
                particle.Play();
            }
        }
    }
}
