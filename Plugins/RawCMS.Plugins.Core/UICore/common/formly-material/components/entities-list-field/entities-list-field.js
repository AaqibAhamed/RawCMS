import { vuexStore } from "/app/config/vuex.js";
import { nameOf } from "/app/utils/object.utils.js";
import { entitiesSchemaService } from "/app/modules/core/services/entities-schema.service.js";
import { BaseField } from "/app/common/formly-material/components/base-field/base-field.js";
import { BaseListFieldDef } from "/app/common/formly-material/components/base-list-field/base-list-field.js";

const _EntitiesListField = async (res, rej) => {
    const baseDef = await BaseListFieldDef();

    res({
        extends: baseDef,
        methods: {
            onChange: function (_e) {
                BaseField.methods.onChange.call(this);
                this.updateRelationMetadata();
            },
            updateRelationMetadata: function () {
                vuexStore.dispatch("core/updateRelationMetadata", {
                    collectionName: this.value
                });
            }
        },
        mounted: async function () {
            const entities = await entitiesSchemaService.getAll();
            this.$set(
                this,
                nameOf(() => this.items),
                entities.map(x => x.CollectionName)
            );
            this.updateRelationMetadata();
        }
    });
};

export const EntitiesListField = _EntitiesListField;
export default _EntitiesListField;