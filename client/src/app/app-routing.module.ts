import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { TestErrorsComponent } from './errors/test-errors/test-errors.component';
import { HomeComponent } from './home/home.component';
import { ListsComponent } from './lists/lists.component';
import { MemberDetailsComponent } from './members/member-details/member-details.component';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MembersListComponent } from './members/members-list/members-list.component';
import { MessagesComponent } from './messages/messages.component';
import { AdminGuard } from './_guard/admin.guard';
import { AuthGuard } from './_guard/auth.guard';
import { PreventUnsavedChangesGuard } from './_guard/prevent-unsaved-changes.guard';
import { MemberDetailsResolver } from './_resolvers/member-details.resolver';

const routes: Routes = [
  {
    path: '', component: HomeComponent
  },
  { 
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      {
        path: 'members', component: MembersListComponent 
      },
      {
        path: 'members/:username', component: MemberDetailsComponent, resolve: {member: MemberDetailsResolver}
      },
      {
        path: 'member/edit/:username', component: MemberEditComponent, canDeactivate: [PreventUnsavedChangesGuard]
      },
      {
        path: 'lists', component: ListsComponent
      },
      {
        path: 'messages', component: MessagesComponent
      },
      {
        path: 'admin', component: AdminPanelComponent, canActivate: [AdminGuard]
      },
    ]
  },
  {path: 'errors', component: TestErrorsComponent},
  {path: 'not-found', component: NotFoundComponent},
  {path: 'server-error', component: ServerErrorComponent},
  {
    path: '**', component: HomeComponent, pathMatch: 'full'
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
