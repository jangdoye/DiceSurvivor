using UnityEngine;

    //사운드 속성 데이터를 관리하는 직렬화된 클래스
    [System.Serializable]
    public class Sound
    {
        //사운드 속성 데이터 이름
        public string name;

        //AudioSource 컴포넌트의 속성
        public AudioClip clip;  //사운드 클립 리소스
        [Range(0f, 1f)]
        public float volume;    //사운드 볼륨 0~1
        [Range(0.1f, 3f)]
        public float pitch;     //사운드 재생속도 
        public bool loop;       //사운드 반복 실행 여부

        public bool isBgm;      //AudioMixer 설정하기 위한 bool 변수

        public bool spitalBlend;   //사운드 위치 설정

        //위의 속성값을 재생할 오디오소스
        public AudioSource source;
    }
