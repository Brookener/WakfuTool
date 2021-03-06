using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace WakfuAudio.Scripts.Classes
{
    public class Psql
    {

        private static string ConnectionPath(string host, string user, string password, string baseName)
        {
            return "Host=" + host + ";Username=" + user + ";Password=" + password + ";Database=" + baseName + ";";
        }
        private static string WakfuConnection = ConnectionPath("opiate.ankama.lan", "wakfu", "wakfu", "wakfu");
        public const string MonsterTable = "tbl_static_monster_characteristics";



        private static bool OpenConnection(NpgsqlConnection connection)
        {
            try
            {
                connection.Open();
            }
            catch(Exception e)
            {
                MessageBox.Show("Impossible to connect to the server\n" + e.Message);
                return false;
            }
            return true;
        }

        //public static List<Monster.Datas> GetMonsterDatas(string id)
        //{
        //    var datas = new List<Monster.Datas>();
        //    var conn = new NpgsqlConnection(WakfuConnection);
        //    if (!OpenConnection(conn))
        //        return datas;
        //    var text = "SELECT * FROM tbl_static_monster_characteristics WHERE monster_gfx_id = " + id + ";";
        //    var cmd = new NpgsqlCommand(text, conn);
        //    NpgsqlDataReader rs = cmd.ExecuteReader();
        //    Monster.Datas data;
        //    while (rs.Read())
        //    {
        //        data = new Monster.Datas()
        //        {
        //            name = rs.GetString(rs.GetOrdinal("monster_gfx_id")),
        //        };
        //        datas.Add(data);
        //    }
        //
        //    return datas;
        //}
        public static Dictionary<int, Monster.Datas> GetMonsterDatas()
        {
            var names = new Dictionary<int, Monster.Datas>();
            var conn = new NpgsqlConnection(WakfuConnection);
            if (!OpenConnection(conn))
                return names;
            var text = "SELECT monster_id as id, monster_admin_name as name, monster_illustration_name as illu FROM tbl_static_monster_characteristics;";
            //var text = "SELECT * FROM tbl_static_monster_characteristics;";
            var cmd = new NpgsqlCommand(text, conn);
            NpgsqlDataReader rs = cmd.ExecuteReader();
            Monster.Datas data;
            while (rs.Read())
            {
                data = new Monster.Datas()
                {
                    id = rs.GetInt32(rs.GetOrdinal("id")),
                    name = rs.GetString(rs.GetOrdinal("name")),
                    dialog = rs.IsDBNull(rs.GetOrdinal("illu")) ? "" : rs.GetString(rs.GetOrdinal("illu")),
                };
                names.Add(data.id, data);
            }

            return names;
        }

    }
    
}
