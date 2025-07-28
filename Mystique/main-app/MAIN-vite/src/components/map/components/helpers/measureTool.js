import mapboxGl from "mapbox-gl";
import MyMapboxDraw from "../myDrawer/index.js";

class MeasureTool {
  /**
   * @type {mapboxGl.Map}
   */
  map;
  toolbar;
  // true 则只能存在一个feature，重新绘制则删除原有
  toolbarForEdit = false;
  onDrawFeature;

  constructor(map, onDrawFeature) {
    this.map = map;
    this.onDrawFeature = onDrawFeature;

    this.map.on("draw.create", this.#draw_create);
    this.map.on("draw.delete", this.#draw_delete);
    this.map.on("draw.update", this.#draw_update);
    this.map.on("draw.modechange", this.#draw_modechange);
  }

  #draw_create = (e) => {
    if (this.onDrawFeature) {
      this.onDrawFeature(e.features);
    }
  };

  #draw_delete = (e) => {
    if (this.onDrawFeature) {
      this.onDrawFeature(undefined);
    }
  };

  #draw_update = (e) => {
    if (this.onDrawFeature) {
      this.onDrawFeature(e.features);
    }
  };

  #draw_modechange = (e) => {
    const autoRemove = [
      "draw_point",
      "draw_angle",
      "draw_line_string",
      "draw_polygon",
    ];
    if (this.toolbarForEdit && autoRemove.includes(e.mode)) {
      const md = this.toolbar.getMode();
      this.toolbar.deleteAll();
      if (this.onDrawFeature) {
        this.onDrawFeature(undefined);
      }
      this.toolbar.changeMode(md);
    }
  };

  isDrawing = () => {
    const autoRemove = [
      "draw_point",
      "draw_angle",
      "draw_line_string",
      "draw_polygon",
    ];
    const md = this.toolbar.getMode();
    return autoRemove.includes(md);
  };

  showToolbar = (items, editMode) => {
    if (items && !this.toolbar) {
      const toolbar = this.#createMeasureTool(items);
      this.map.addControl(toolbar);
      this.toolbar = toolbar;
      this.toolbarForEdit = editMode;
    }
    if (!items && this.toolbar) {
      this.map.removeControl(this.toolbar);
      this.toolbar = undefined;
    }
  };

  dispose = () => {
    this.map.off("draw.create", this.#draw_create);
    this.map.off("draw.delete", this.#draw_delete);
    this.map.off("draw.update", this.#draw_update);
    this.map.off("draw.modechange", this.#draw_modechange);
  };

  #createMeasureTool = (items, theme = "dark") => {
    const mmd = new MyMapboxDraw({
      touchEnabled: true,
      displayControlsDefault: false,
      theme,
      controls: items,
      // controls: editMode
      //   ? {
      //       polygon: "绘面",
      //       point: "标点",
      //     }
      //   : {
      //       polygon: "面积",
      //       line_string: "测距",
      //       point: "标点",
      //       trash: "删除",
      //       measuring_angle: "角度",
      //     },
    });
    return mmd;
  };
}

export default MeasureTool;
