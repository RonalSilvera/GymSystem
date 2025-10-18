import { createReducer, on, Action } from '@ngrx/store'
import * as reduxActions from '../actions'

const initialState = false

const setLoadingVisible = createReducer(
    initialState,
    on( reduxActions.setLoadingVisible, (state, { loadingVisible }) => loadingVisible  ),
)

export function loadingVisible(state: any, action: Action) {
    return setLoadingVisible(state, action)
}
