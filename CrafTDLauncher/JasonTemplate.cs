using System.Collections.Generic;

public class Share
{
    public bool is_root { get; set; }
    public bool is_owned { get; set; }
    public string rights { get; set; }
}

public class Owner
{
    public string login { get; set; }
    public string display_name { get; set; }
    public string uid { get; set; }
}

public class Embedded
{
    public string sort { get; set; }
    public string public_key { get; set; }
    public List<RootObject> items { get; set; }
    public int limit { get; set; }
    public int offset { get; set; }
    public string path { get; set; }
    public int total { get; set; }
}

public class Exif
{
    public string date_time { get; set; }
}

public class CommentIds
{
    public string private_resource { get; set; }
    public string public_resource { get; set; }
}

public class RootObject
{
    public int views_count { get; set; }
    public string resource_id { get; set; }
    public Share share { get; set; }
    public string file { get; set; }
    public Owner owner { get; set; }
    public long size { get; set; }
    public string photoslice_time { get; set; }
    public Embedded _embedded { get; set; }
    public Exif exif { get; set; }
    public string media_type { get; set; }
    public string sha256 { get; set; }
    public string type { get; set; }
    public string mime_type { get; set; }
    public long revision { get; set; }
    public string public_url { get; set; }
    public string path { get; set; }
    public string md5 { get; set; }
    public string public_key { get; set; }
    public string preview { get; set; }
    public string name { get; set; }
    public string created { get; set; }
    public string modified { get; set; }
    public CommentIds comment_ids { get; set; }
}