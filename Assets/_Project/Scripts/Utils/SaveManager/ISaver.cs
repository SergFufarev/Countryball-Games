using System;

namespace FunnyBlox
{
    //[Obsolete] // todo в будущем сделать сохранения центролизованными а не разбросанными по скриптам. При этом учесть, чтобы данные не потерялись у старых игроков!
    public interface ISaver
    {
        public void SaveData();
        public void LoadData();
    }
}