//
//3 lines taken and modified from https://answers.unity.com/questions/254130/how-do-i-rotate-an-object-towards-a-vector3-point.html in regards to rotating over time to look at a target

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class TrigMissile : MonoBehaviour
{
    [SerializeField] private GameObject _flames;
    [SerializeField] private float _distanceToFirstWaypoint = 20f;
    [SerializeField] private float _rotationSpeed = 1;
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] private float _curveSpeed = 1;

    private bool _launched = false;
    private bool _reachedWaypointOne = false;
    private Vector3 _waypointOne = Vector3.zero;
    private Vector3 _currentTarget = Vector3.zero;
    private Transform _finalTarget;
    private float speed = 0;
    private Vector3 _direction;
    private Quaternion _lookRotation;

    public void Launch(Transform target)
    {
        _finalTarget = target;
        _flames.SetActive(true);
        StartCoroutine(TweenSpeed());
        _launched = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        _waypointOne = transform.TransformPoint(Vector3.forward * _distanceToFirstWaypoint);
        _currentTarget = _waypointOne;
        speed = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(_launched)
        {
            if(_reachedWaypointOne) //do rotation to final target
            {
                //find the vector pointing from our position to the target
                _direction = (_currentTarget - transform.position).normalized;

                //create the rotation we need to be in to look at the target
                _lookRotation = Quaternion.LookRotation(_direction);

                //rotate us over time according to speed until we are in the required rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * _rotationSpeed);
            }
            
            transform.position = Vector3.MoveTowards(transform.position, _currentTarget, speed * Time.deltaTime);
            
            if(Vector3.Distance(transform.position, _currentTarget) < 0.0001f)
            {
                if(_reachedWaypointOne)
                {
                    //previously reached the first waypoint - so we must have also reached final target
                    //destroy missile and target
                    TrigQuestionManager.Instance.FiringSequenceComplete();
                    
                    Destroy(this.gameObject);
                }
                else
                {
                    _reachedWaypointOne = true;
                    _currentTarget = _finalTarget.position;
                }
                

            }
        }
    }

    IEnumerator TweenSpeed()
    {
        speed = 0;
        float t = 0;
        float maxT = _speedCurve.keys[_speedCurve.length - 1].time;

        while (t < maxT)
        {
            t += _curveSpeed * Time.deltaTime;

            speed = _speedCurve.Evaluate(t);

            yield return null;
        }
    }
}
