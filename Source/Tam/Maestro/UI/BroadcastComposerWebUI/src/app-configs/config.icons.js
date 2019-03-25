import { library } from "@fortawesome/fontawesome-svg-core";

// light
import {
  faSpinnerThird as falSpinnerThird,
  faCircle as falCircle
} from "@fortawesome/pro-light-svg-icons";

// solid
import {
  faCircle as fasCircle,
  faCheckCircle as fasCheckCircle,
  faTimeCircle as fasTimeCircle,
  faBolt as fasBolt
} from "@fortawesome/pro-solid-svg-icons";

// solid
import {
  faCircle as farTimeCircle,
  faCheckCircle as farCheckCircle
} from "@fortawesome/pro-regular-svg-icons";

export const configureIcons = () => {
  // light
  library.add(falSpinnerThird);
  library.add(falCircle);
  // solid
  library.add(fasCircle);
  library.add(fasBolt);
  library.add(fasCheckCircle);
  library.add(fasTimeCircle);
  // refular
  library.add(farCheckCircle);
  library.add(farTimeCircle);
};
