import { library } from "@fortawesome/fontawesome-svg-core";

// light
import {
  faSpinnerThird as falSpinnerThird,
  faQuestionCircle as falQuestionCircle,
  faEllipsisH as falEllipsisH,
  faTimes as falTimes,
  faCircle as falCircle
} from "@fortawesome/pro-light-svg-icons";

// solid
import {
  faCircle as fasCircle,
  faCheckCircle as fasCheckCircle,
  faTimesCircle as fasTimesCircle,
  faBolt as fasBolt
} from "@fortawesome/pro-solid-svg-icons";

// solid
import {
  faTimesCircle as farTimesCircle,
  faCheckCircle as farCheckCircle
} from "@fortawesome/pro-regular-svg-icons";

export const configureIcons = () => {
  // light
  library.add(
    falSpinnerThird,
    falCircle,
    falQuestionCircle,
    falEllipsisH,
    falTimes
  );
  // solid
  library.add(fasCircle, fasBolt, fasCheckCircle, fasTimesCircle);
  // refular
  library.add(farCheckCircle, farTimesCircle);
};
