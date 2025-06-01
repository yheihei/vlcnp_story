## PRD: Projectileシステムのインターフェース化

### 背景
現在のProjectileクラスは具象クラスとして実装されており、異なる物理的な動きを持つProjectileを追加する際の拡張性に欠けています。将来的に様々な動きのProjectileを実装するための基盤として、インターフェースを導入する必要があります。

### 目的
- Projectileシステムにインターフェースを導入し、拡張可能な設計にする
- 既存のコードでProjectileで参照しているものは、インターフェースに置き換える
  - ユーザーがあとで再アタッチする
- 将来の新しいProjectileタイプの追加を容易にする

### 要件

#### 1. IProjectileインターフェースの作成
以下のメンバーを持つインターフェースを作成する：
```csharp
namespace VLCNP.Combat
{
    public interface IProjectile
    {
        // プロパティ
        bool IsStucking { get; }
        
        // メソッド
        void SetDirection(bool isLeft);
        void SetDamage(float damage);
        void ImpactAndDestroy();
    }
}
```

#### 2. 既存Projectileクラスの修正
- IProjectileインターフェースを実装する
- 既存の動作は一切変更しない（後方互換性を完全に維持）
- クラス宣言を以下のように変更：
  ```csharp
  public class Projectile : MonoBehaviour, IStoppable, IProjectile
  ```

#### 3. Shield.csの修正
現在のコード：
```csharp
Projectile projectile = other.GetComponent<Projectile>();
if (projectile == null) return;
if (projectile.IsStucking) return;
projectile.ImpactAndDestroy();
```

修正後のコード：
```csharp
// 新しいIProjectile実装クラスに対応
IProjectile iProjectile = other.GetComponent<IProjectile>();
if (iProjectile != null)
{
    if (iProjectile.IsStucking) return;
    iProjectile.ImpactAndDestroy();
}
```

#### 4. WeaponConfig.csの修正
WeaponLevelクラスのprojectileフィールドをIProjectile型に変更：

```csharp
[System.Serializable]
class WeaponLevel
{
    [SerializeField]
    public float damage;

    [SerializeField]
    public GameObject projectilePrefab; // IProjectileを実装したGameObject
}
```

HasProjectileメソッドの修正：
```csharp
public bool HasProjectile(int level = 1)
{
    WeaponLevel _weaponLevel = GetCurrentWeapon(level);
    if (_weaponLevel == null)
        return false;
    return _weaponLevel.projectilePrefab != null;
}
```

LaunchProjectileメソッドの修正：
```csharp
public void LaunchProjectile(Transform handTransform, int level = 1, bool isLeft = false)
{
    WeaponLevel _weaponLevel = GetCurrentWeapon(level);
    GameObject projectileObj = Instantiate(
        _weaponLevel.projectilePrefab,
        handTransform.position,
        handTransform.rotation
    );
    
    // 音声処理
    AudioClip clip = projectileObj.GetComponent<AudioSource>()?.clip;
    if (clip != null)
    {
        AudioSource.PlayClipAtPoint(clip, handTransform.position);
    }
    
    // IProjectileインターフェースを通じて操作
    IProjectile projectile = projectileObj.GetComponent<IProjectile>();
    if (projectile != null)
    {
        projectile.SetDirection(isLeft);
        projectile.SetDamage(_weaponLevel.damage);
    }
}
```

### 制約事項
- 既存のPrefabは再設定が必要（ユーザーが手動で対応）
- ゲームプレイに一切影響を与えない
- どのコンポーネントを再アタッチする必要があるかの情報を設計書に含めること

### テスト項目
1. 既存のProjectileが従来通り動作すること
2. Shield.csが既存のProjectileを正しく処理できること
3. WeaponConfigから既存のProjectileが正しく発射されること
4. すべての既存の武器が正常に動作すること