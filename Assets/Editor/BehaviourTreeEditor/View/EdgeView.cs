using System;
using Cysharp.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace JLBehaviourTree.Editor.View
{
    public enum MovePointState
    {
        停止的, 运行的, 暂停的
    }

    public class EdgeView : Edge
    {
        private VisualElement[] _movePoints;

        private MovePointState _isMoveState;

        private int _stepIndex;

        private int _pointNumber = 4;

        public EdgeView() : base()
        {
            _movePoints = new VisualElement[_pointNumber];
        }
        public void ChangeEdgeColor(Color color)
        {
            edgeControl.inputColor = color;
            edgeControl.outputColor = color;
            edgeControl.UpdateLayout();
        }

        public void OnStartMovePoints()
        {
            if (_isMoveState == MovePointState.运行的) return;
            if (_isMoveState == MovePointState.停止的)
            {
                _pointNumber = (int)Math.Max(GetEdgeLength() / 30, 1);//根据边的长度计算移动点的数量
                _movePoints = new VisualElement[_pointNumber];
                for (int i = 0; i < _pointNumber; i++)
                {
                    _movePoints[i] = new MovePoint();
                    Add(_movePoints[i]);
                }
                _stepIndex = 0;
                // ChangeEdgeColor(Color.red);
            }
            _isMoveState = MovePointState.运行的;
            MovePoints();
        }

        async void MovePoints()
        {
            while (_isMoveState == MovePointState.运行的)
            {
                _stepIndex = _stepIndex % 100;
                for (int i = 0; i < _pointNumber; i++)
                {
                    float t = (_stepIndex / 100f) + (float)i / _pointNumber;
                    t = t > 1 ? t - 1 : t;
                    _movePoints[i].transform.position = Vector2.Lerp(edgeControl.controlPoints[1],
                        edgeControl.controlPoints[2], t);
                }

                _stepIndex++;
                await UniTask.Delay(10);
            }

        }

        public void OnSuspendMovePoints() => _isMoveState = MovePointState.暂停的;

        public void OnStopMovePoints()
        {
            if (_isMoveState == MovePointState.停止的) return;
            _isMoveState = MovePointState.停止的;
            // ChangeEdgeColor(Color.black);
            for (int i = _movePoints.Length - 1; i >= 0; i--)
            {
                if (_movePoints[i] != null)
                {
                    this.Remove(_movePoints[i]);
                }

            }
        }
        public float GetEdgeLength()
        {
            if (edgeControl.controlPoints.Length < 2)
            {
                return 0f;
            }

            Vector2 startPoint = edgeControl.controlPoints[0];
            Vector2 endPoint = edgeControl.controlPoints[edgeControl.controlPoints.Length - 1];
            return Vector2.Distance(startPoint, endPoint);
        }
    }
}


