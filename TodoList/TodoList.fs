// Copyright Fabulous contributors. See LICENSE.md for license.
namespace TodoList

// open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms

module App =
    type Todo = string

    type Model = { Todos: Todo list; EntryText: string }

    type Msg =
        | AddTodo of Todo: string
        | RemoveTodo of Todo: string
        | UpdateTextEntry of string

    let initModel =
        { Todos =
              [ "Buy better lifting gloves"
                "Write ruby class" ]
          EntryText = "" }

    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | AddTodo todo ->
            { model with
                  Todos = todo :: model.Todos
                  EntryText = "" },
            Cmd.none
        | RemoveTodo todo ->
            { model with
                  Todos = model.Todos |> List.filter (fun e -> e = todo) },
            Cmd.none
        | UpdateTextEntry text -> { model with EntryText = text }, Cmd.none

    //    let TextChanged oldTextValue newTextValue =

    let view (model: Model) dispatch =
        View.ContentPage
            (title = "Todo List",
             content =
                 View.StackLayout
                     (padding = Thickness 20.0,
                      verticalOptions = LayoutOptions.Center,
                      children =
                          [ View.Entry
                              (text = model.EntryText,
                               placeholder = "Add a todo",
                               placeholderColor = Color.Gray,
                               textChanged = (fun args -> dispatch (Msg.UpdateTextEntry args.NewTextValue)),
                               completed = (fun text -> dispatch (Msg.AddTodo text)))
                            View.ListView
                                ([ for item in model.Todos ->
                                    View.ViewCell(View.Label(text = item, textColor = Color.Blue)) ]) ]))

    // Note, this declaration is needed if you enable LiveUpdate
    let program =
        XamarinFormsProgram.mkProgram init update view
#if DEBUG
        |> Program.withConsoleTrace
#endif

type App() as app =
    inherit Application()

    let runner =
        App.program |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode.
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
//
    do runner.EnableLiveUpdate()
#endif

// Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"

    override __.OnSleep() =

        let json =
            Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)

        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() =
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) ->

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)

                let model =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel(model, Cmd.none)

            | _ -> ()
        with ex -> App.program.onError ("Error while restoring model found in app.Properties", ex)

    override this.OnStart() =
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif
