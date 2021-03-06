﻿module Foundation.Test.ViewModels {
    @Core.SecureFormViewModelDependency({
        name: "NestedRouteMainFormViewModel", templateUrl: "|Foundation|/Foundation.Test.HtmlClient/views/tests/nestedRouteMainview.html",
        $routeConfig: [
            { path: "/first-part-page/", name: "FirstPartFormViewModel", useAsDefault: true },
            { path: "/second-part-page/:parameter", name: "SecondPartFormViewModel" },
            { path: "/**", redirectTo: ["NestedRouteMainFormViewModel"] }
        ]
    })
    export class NestedRouteMainFormViewModel extends ViewModel.ViewModels.FormViewModel {
        public constructor( @Core.Inject("$document") public $document: ng.IDocumentService) {
            super();
            this.$document.attr("title", "Nested View");
        }
    }

    @Core.SecureFormViewModelDependency({ name: "FirstPartFormViewModel", templateUrl: "|Foundation|/Foundation.Test.HtmlClient/views/tests/firstPartview.html" })
    export class FirstPartFormViewModel extends ViewModel.ViewModels.FormViewModel {

        public goToNextPart(): void {
            this.$router.navigate(["SecondPartFormViewModel", { parameter: 1 }]);
        }
    }

    @Core.SecureFormViewModelDependency({
        name: "SecondPartFormViewModel", templateUrl: "|Foundation|/Foundation.Test.HtmlClient/views/tests/secondPartview.html"
    })
    export class SecondPartFormViewModel extends ViewModel.ViewModels.FormViewModel {

        public parameter: number;

        public async $onInit(): Promise<void> {
            this.parameter = this.route.params["parameter"];
        }
    }
}