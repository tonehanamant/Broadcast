This comnponent is a general wrapper for the `<FontAwesomeIcon />` component released by the people at FontAwesome. This component has access to the Pro license so all regular, heavy, and light icon sets can be utilized.

Check out the selection of icons: [FontAwesomeIcons](https://fontawesome.com/icons)

Import an icon from the icon index file at the root of the directory. In this case, a webpack import alias has already been created. All that needs to be done is the object needs to be imported.

```jsx static
import { lightIcons } from FAIcons';
const icon = lightIcons.faRocket;        // can be whatever icon that exists in the FontAwesome Icon repo
```

**Default Icons**

```jsx
<div
  style={{
    display: "flex",
    flexDirection: "row",
    justifyContent: "flex-start"
  }}
>
  <Icon icon={"rocket"} iconSize="xs" iconColor="default" />
  <Icon icon={"rocket"} iconSize="sm" iconColor="default" />
  <Icon icon={"rocket"} iconSize="md" iconColor="default" />
  <Icon icon={"rocket"} iconSize="lg" iconColor="default" />
  <Icon icon={"rocket"} iconSize="xl" iconColor="default" />
  <Icon icon={"rocket"} iconSize="xxl" iconColor="default" />
</div>
```

**Color Icons**

```jsx
const icons = require("FAIcons");
const { lightIcons } = icons;
<div
  style={{
    display: "flex",
    flexDirection: "row",
    justifyContent: "flex-start"
  }}
>
  <Icon icon={lightIcons.faEyeSlash} iconSize="xs" iconColor="default" />
  <Icon icon={lightIcons.faEyeSlash} iconSize="sm" iconColor="success" />
  <Icon icon={lightIcons.faEyeSlash} iconSize="md" iconColor="primary" />
  <Icon icon={lightIcons.faEyeSlash} iconSize="lg" iconColor="gray-1" />
  <Icon icon={lightIcons.faEyeSlash} iconSize="xl" iconColor="invalid" />
  <Icon icon={lightIcons.faEyeSlash} iconSize="xxl" iconColor="tertiary" />
</div>;
```
