// 接收任意要素，计算并返回该要素的缓冲区 GeoJSON
import buffer from "@turf/buffer";
import { polygon, point } from "@turf/helpers";
// 接收入参要素(Feature)或要素集(FeatureCollection)，计算并返回它们的矩心
import centroid from "@turf/centroid";
// 接收一个点要素和一个面要素，判断点要素是否在面要素内
import booleanPointInPolygon from "@turf/boolean-point-in-polygon";
// 接收一个面要素和点要素(集合)，计算并返回在该面要素内部的点
import pointsWithinPolygon from "@turf/points-within-polygon";
// 接收两个 type 为 Polygon 的多边形要素，返回第一个多边形裁剪第二个多边形后得到的要素
// 值得注意的是，裁剪得到的要素的 properties 与第一个要素的相同
import difference from "@turf/difference";
import destination from "@turf/destination";
import circle from "@turf/circle";

const myTurf = {
  buffer,
  polygon,
  point,
  centroid,
  booleanPointInPolygon,
  pointsWithinPolygon,
  difference,
  destination,
  circle,
};

export default myTurf;
