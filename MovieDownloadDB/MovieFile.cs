using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieDownloadDB
{
    class MovieFile
    {

        public MovieFile(String name,String time,long size,String path)
        {
            _name = name;
            _time = time;
            _path = path;
            _size = size;
        }

        private String _name;

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private String _time;

        public String Time
        {
            get { return _time; }
            set { _time = value; }
        }

        private String _path;

        public String Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private long _size;

        public long Size
        {
            get { return _size; }
            set { _size = value; }
        }


    }
}
