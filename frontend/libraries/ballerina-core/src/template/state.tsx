import { BasicFun } from "../fun/state";
import { BasicUpdater } from "../fun/domains/updater/state";
import { Unit, unit } from "../../main";

export type View<context, state, foreignMutations, embeddedChildren = Unit> = (
  props: HeadlessTemplateProps<context, state, foreignMutations> &
    embeddedChildren,
) => JSX.Element;

export type HeadlessTemplateProps<context, state, foreignMutations> = {
  context: context;
  setState: BasicFun<BasicUpdater<state>, void>;
  foreignMutations: foreignMutations;
};

export type TemplateChildren = JSX.Element | JSX.Element[];

export type Wrapper = BasicFun<{ children?: TemplateChildren }, JSX.Element>;

export type TemplateProps<context, state, foreignMutations, view = Unit> = {
  context: context;
  setState: BasicFun<BasicUpdater<state>, void>;
  foreignMutations: foreignMutations;
  view: view;
  children?: TemplateChildren;
};

type TemplateRunner<context, state, foreignMutations, view = Unit> = (
  props: TemplateProps<context, state, foreignMutations, view>,
) => JSX.Element;

export type Template<context, state, foreignMutations, view = Unit> = {
  any: BasicFun<
    Array<Template<context, state, foreignMutations>>,
    Template<context, state, foreignMutations, view>
  >;
  wrap: (Wrapper: Wrapper) => Template<context, state, foreignMutations, view>;
  withView: (view: view) => Template<context, state, foreignMutations, Unit>;
  withViewFromProps: (
    f: BasicFun<TemplateProps<context, state, foreignMutations, Unit>, view>,
  ) => Template<context, state, foreignMutations, Unit>;
  mapState: <newState>(
    f: BasicFun<BasicUpdater<state>, BasicUpdater<newState>>,
  ) => Template<context, newState, foreignMutations, view>;
  mapStateFromProps: <newState>(
    f: BasicFun<
      [
        TemplateProps<context, newState, foreignMutations, view>,
        BasicUpdater<state>,
      ],
      BasicUpdater<newState>
    >,
  ) => Template<context, newState, foreignMutations, view>;
  mapContext: <newContext>(
    f: BasicFun<newContext, context | undefined>,
  ) => Template<newContext, state, foreignMutations, view>;
  mapForeignMutations: <newForeignMutations>(
    f: BasicFun<newForeignMutations, foreignMutations>,
  ) => Template<context, state, newForeignMutations, view>;
  mapContextFromProps: <newContext>(
    f: BasicFun<
      TemplateProps<newContext, state, foreignMutations, view>,
      context | undefined
    >,
  ) => Template<newContext, state, foreignMutations, view>;
  mapForeignMutationsFromProps: <newForeignMutations>(
    f: BasicFun<
      TemplateProps<context, state, newForeignMutations, view>,
      foreignMutations
    >,
  ) => Template<context, state, newForeignMutations, view>;
  mapView: <newView>(
    f: BasicFun<newView, view>,
  ) => Template<context, state, foreignMutations, newView>;
  mapViewFromProps: <newView>(
    f: BasicFun<TemplateProps<context, state, foreignMutations, newView>, view>,
  ) => Template<context, state, foreignMutations, newView>;
} & TemplateRunner<context, state, foreignMutations, view>;

export const createTemplate = <context, state, foreignMutations, view>(
  actual: TemplateRunner<context, state, foreignMutations, view>,
): Template<context, state, foreignMutations, view> => {
  const result = actual as Template<context, state, foreignMutations, view>;
  result.wrap = function (
    this: Template<context, state, foreignMutations, view>,
    Wrapper: Wrapper,
  ) {
    return Template.Operations.Wrap(this, Wrapper);
  };
  result.withView = function (
    this: Template<context, state, foreignMutations, view>,
    view: view,
  ): Template<context, state, foreignMutations, Unit> {
    return Template.Operations.WithView(this, view);
  };
  result.withViewFromProps = function (
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<TemplateProps<context, state, foreignMutations, Unit>, view>,
  ): Template<context, state, foreignMutations, Unit> {
    return Template.Operations.WithViewFromProps(this, f);
  };
  result.mapState = function <newState>(
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<BasicUpdater<state>, BasicUpdater<newState>>,
  ): Template<context, newState, foreignMutations, view> {
    return Template.Operations.MapState(this, f);
  };
  result.mapStateFromProps = function <newState>(
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<
      [
        TemplateProps<context, newState, foreignMutations, view>,
        BasicUpdater<state>,
      ],
      BasicUpdater<newState>
    >,
  ): Template<context, newState, foreignMutations, view> {
    return Template.Operations.MapStateFromProps(this, f);
  };
  result.mapContext = function <newContext>(
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<newContext, context | undefined>,
  ) {
    return Template.Operations.MapContext(this, f);
  };
  result.mapForeignMutations = function <newForeignMutations>(
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<newForeignMutations, foreignMutations>,
  ): Template<context, state, newForeignMutations, view> {
    return Template.Operations.MapForeignMutations(this, f);
  };
  result.mapContextFromProps = function <newContext>(
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<
      TemplateProps<newContext, state, foreignMutations, view>,
      context | undefined
    >,
  ) {
    return Template.Operations.MapContextFromProps(this, f);
  };
  result.mapForeignMutationsFromProps = function <newForeignMutations>(
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<
      TemplateProps<context, state, newForeignMutations, view>,
      foreignMutations
    >,
  ): Template<context, state, newForeignMutations, view> {
    return Template.Operations.MapForeignMutationsFromProps(this, f);
  };
  result.mapView = function <newView>(
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<newView, view>,
  ): Template<context, state, foreignMutations, newView> {
    return Template.Operations.MapView(this, f);
  };
  result.mapViewFromProps = function <newView>(
    this: Template<context, state, foreignMutations, view>,
    f: BasicFun<TemplateProps<context, state, foreignMutations, newView>, view>,
  ): Template<context, state, foreignMutations, newView> {
    return Template.Operations.MapViewFromProps(this, f);
  };
  result.any = function (
    this: Template<context, state, foreignMutations, view>,
    others: Array<Template<context, state, foreignMutations>>,
  ): Template<context, state, foreignMutations, view> {
    return Template.Operations.Any(this, others);
  };
  return result;
};

export const Template = {
  Default: <context, state, foreignMutations, view = Unit>(
    actual: TemplateRunner<context & state, state, foreignMutations, view>,
  ): Template<context & state, state, foreignMutations, view> =>
    createTemplate(actual),
  Operations: {
    Wrap: <context, state, foreignMutations, view>(
      p: Template<context, state, foreignMutations, view>,
      Wrapper: BasicFun<{ children?: TemplateChildren }, JSX.Element>,
    ): Template<context, state, foreignMutations, view> =>
      createTemplate((props) => <Wrapper>{p(props)}</Wrapper>),
    WithView: <context, state, foreignMutations, view>(
      p: Template<context, state, foreignMutations, view>,
      view: view,
    ): Template<context, state, foreignMutations, Unit> =>
      createTemplate((props) => (
        <>
          {p({
            ...props,
            view: view,
          })}
        </>
      )),
    WithViewFromProps: <context, state, foreignMutations, view>(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<TemplateProps<context, state, foreignMutations, Unit>, view>,
    ): Template<context, state, foreignMutations, Unit> =>
      createTemplate((props) => (
        <>
          {p({
            ...props,
            view: f(props),
          })}
        </>
      )),
    MapState: <context, state, newState, foreignMutations, view>(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<BasicUpdater<state>, BasicUpdater<newState>>,
    ): Template<context, newState, foreignMutations, view> =>
      createTemplate((props) => (
        <>
          {p({
            context: props.context,
            setState: (_) => props.setState(f(_)),
            foreignMutations: props.foreignMutations,
            view: props.view,
          })}
        </>
      )),
    MapStateFromProps: <context, state, newState, foreignMutations, view>(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<
        [
          TemplateProps<context, newState, foreignMutations, view>,
          BasicUpdater<state>,
        ],
        BasicUpdater<newState>
      >,
    ): Template<context, newState, foreignMutations, view> =>
      createTemplate((props) => (
        <>
          {p({
            context: props.context,
            setState: (_) => props.setState(f([props, _])),
            foreignMutations: props.foreignMutations,
            view: props.view,
          })}
        </>
      )),
    MapContext: <context, newContext, state, foreignMutations, view>(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<newContext, context | undefined>,
    ): Template<newContext, state, foreignMutations, view> =>
      createTemplate((props) => {
        const context = f(props.context);
        if (!context) return <></>;
        return (
          <>
            {p({
              context: context,
              setState: props.setState,
              foreignMutations: props.foreignMutations,
              view: props.view,
            })}
          </>
        );
      }),
    MapForeignMutations: <
      context,
      state,
      foreignMutations,
      newForeignMutations,
      view,
    >(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<newForeignMutations, foreignMutations>,
    ): Template<context, state, newForeignMutations, view> =>
      createTemplate((props) => (
        <>
          {p({
            context: props.context,
            setState: props.setState,
            foreignMutations: f(props.foreignMutations),
            view: props.view,
          })}
        </>
      )),

    MapContextFromProps: <context, newContext, state, foreignMutations, view>(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<
        TemplateProps<newContext, state, foreignMutations, view>,
        context | undefined
      >,
    ): Template<newContext, state, foreignMutations, view> =>
      createTemplate((props) => {
        const context = f(props);
        if (!context) return <></>;
        return (
          <>
            {p({
              context: context,
              setState: props.setState,
              foreignMutations: props.foreignMutations,
              view: props.view,
            })}
          </>
        );
      }),
    MapForeignMutationsFromProps: <
      context,
      state,
      foreignMutations,
      newForeignMutations,
      view,
    >(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<
        TemplateProps<context, state, newForeignMutations, view>,
        foreignMutations
      >,
    ): Template<context, state, newForeignMutations, view> =>
      createTemplate((props) => (
        <>
          {p({
            context: props.context,
            setState: props.setState,
            foreignMutations: f(props),
            view: props.view,
          })}
        </>
      )),
    Any: <context, state, foreignMutations, view>(
      p: Template<context, state, foreignMutations, view>,
      others: Array<Template<context, state, foreignMutations>>,
    ): Template<context, state, foreignMutations, view> =>
      createTemplate((props) => {
        return (
          <>
            {[
              p(props),
              ...others.map((_) =>
                _({
                  context: props.context,
                  setState: props.setState,
                  foreignMutations: props.foreignMutations,
                  view: unit,
                }),
              ),
            ]}
          </>
        );
      }),
    MapView: <context, state, foreignMutations, newView, view>(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<newView, view>,
    ): Template<context, state, foreignMutations, newView> =>
      createTemplate((props) => {
        const view = f(props.view);
        return (
          <>
            {p({
              context: props.context,
              setState: props.setState,
              foreignMutations: props.foreignMutations,
              view: view,
            })}
          </>
        );
      }),
    MapViewFromProps: <context, state, foreignMutations, newView, view>(
      p: Template<context, state, foreignMutations, view>,
      f: BasicFun<
        TemplateProps<context, state, foreignMutations, newView>,
        view
      >,
    ): Template<context, state, foreignMutations, newView> =>
      createTemplate((props) => {
        const view = f(props);
        return (
          <>
            {p({
              context: props.context,
              setState: props.setState,
              foreignMutations: props.foreignMutations,
              view: view,
            })}
          </>
        );
      }),
  },
};
