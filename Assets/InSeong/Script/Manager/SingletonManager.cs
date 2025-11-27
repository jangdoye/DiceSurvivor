using UnityEngine;

//싱글톤의 토대가 되는 부모 클래스
namespace DiceSurvivor.Manager {
    //추상 글래스로 선언 : 직접 인스턴스 사용을 하지 않고, 상속을 원활하게 하기 위해
    public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour {
        #region Variables
        static T instance;
        //단일 스레드 동작을 위한 lock용 더미
        static readonly object lockObject = new();
        //게임 끄는 도중 호출 방지를 위한 플래그
        static bool isQuitting = false;

        #endregion

        #region Properties
        public static T Instance {
            get {
                //끄는 도중에는 반환하지 않는다
                if (isQuitting) return null;
                //스레드 동기화 - 다른 스레드에서 lockObject를 사용하고 있지 않다면 실행
                lock (lockObject)
                {
                    //인스턴스가 없다면 찾아서 할당
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<T>();
                        //찾아서 없으면 생성하고 이름을 T로 할당
                        if (instance == null)
                        {
                            GameObject singleton = new();
                            instance = singleton.AddComponent<T>();
                            singleton.name = typeof(T).ToString();
                        }
                    }
                    
                }
                return instance;
            }
        }
        #endregion

        #region Unity Event Method
        //가상 메서드 : 상속받은 애들의 오버라이딩 지원
        protected virtual void Awake() {
            if (instance == null)
            {
                instance = this as T;
                //DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if(instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        protected virtual void OnApplicationQuit() {
            isQuitting = true;
        }
        #endregion

        #region Custom Method
        protected virtual void OnSingletonAwake() {
            //TODO : 상속받은 클래스에서 초기화 시 별도 수행할 내용이 있으면 수행
        }
        #endregion
    }

}
